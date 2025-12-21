import { sql } from "@vercel/postgres";

function sendJson(res, statusCode, body) {
  res.statusCode = statusCode;
  res.setHeader("Content-Type", "application/json; charset=utf-8");
  res.end(JSON.stringify(body));
}

function parseBody(req) {
  if (req.body == null) return null;
  if (typeof req.body === "object") return req.body;
  if (typeof req.body === "string" && req.body.length > 0) {
    try {
      return JSON.parse(req.body);
    } catch {
      return null;
    }
  }
  return null;
}

function toInt(value) {
  if (typeof value === "number" && Number.isFinite(value)) return Math.trunc(value);
  if (typeof value === "string" && value.trim().length > 0) {
    const n = Number(value);
    if (Number.isFinite(n)) return Math.trunc(n);
  }
  return null;
}

export default async function handler(req, res) {
  // Defensive preflight handling (not required for same-origin, but harmless).
  if (req.method === "OPTIONS") {
    res.statusCode = 204;
    res.setHeader("Allow", "POST, OPTIONS");
    res.end();
    return;
  }

  if (req.method !== "POST") {
    res.statusCode = 405;
    res.setHeader("Allow", "POST, OPTIONS");
    sendJson(res, 405, { error: "Method Not Allowed" });
    return;
  }

  const body = parseBody(req);
  if (!body) {
    sendJson(res, 400, { error: "Invalid JSON body" });
    return;
  }

  const messageRaw = typeof body.message === "string" ? body.message : "";
  const message = messageRaw.trim();
  if (message.length === 0) {
    sendJson(res, 400, { error: "message is required" });
    return;
  }
  if (message.length > 4000) {
    sendJson(res, 400, { error: "message too long" });
    return;
  }

  const run = toInt(body.run);
  const day = toInt(body.day);
  if (run == null || run < 1 || run > 999) {
    sendJson(res, 400, { error: "run must be an integer >= 1" });
    return;
  }
  if (day == null || day < 1 || day > 999) {
    sendJson(res, 400, { error: "day must be an integer >= 1" });
    return;
  }

  const buildVersion = typeof body.buildVersion === "string" ? body.buildVersion : null;
  const nodeName = typeof body.nodeName === "string" ? body.nodeName : null;
  const pageUrl = typeof body.pageUrl === "string" ? body.pageUrl : null;
  const userAgent = typeof req.headers?.["user-agent"] === "string" ? req.headers["user-agent"] : null;

  try {
    await sql`
      INSERT INTO feedback (message, run, day, build_version, node_name, page_url, user_agent)
      VALUES (${message}, ${run}, ${day}, ${buildVersion}, ${nodeName}, ${pageUrl}, ${userAgent})
    `;

    // Keep response tiny for Unity.
    res.statusCode = 204;
    res.end();
  } catch (err) {
    console.error("feedback insert failed:", err);
    sendJson(res, 500, { error: "Internal Server Error" });
  }
}


