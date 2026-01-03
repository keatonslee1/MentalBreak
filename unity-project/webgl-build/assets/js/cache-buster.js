/**
 * Cache Buster for Mental Break
 *
 * This script checks if a new version has been deployed and forces
 * a complete cache clear + reload when necessary.
 *
 * How it works:
 * 1. Fetches version.json with a timestamp to bypass CDN cache
 * 2. Compares version with localStorage
 * 3. If different: clears IndexedDB (Unity PlayerPrefs) and hard reloads
 * 4. If same: continues loading normally
 */
(function() {
  'use strict';

  var STORAGE_KEY = 'mb_version';
  var VERSION_URL = '/version.json';

  // Check if we're in a reload loop (prevent infinite reloads)
  var RELOAD_FLAG = 'mb_just_reloaded';
  if (sessionStorage.getItem(RELOAD_FLAG)) {
    sessionStorage.removeItem(RELOAD_FLAG);
    console.log('[CacheBuster] Post-reload check - skipping to prevent loop');
    return;
  }

  /**
   * Clear all Unity-related storage
   */
  function clearUnityCache() {
    return new Promise(function(resolve) {
      var cleared = [];

      // Clear PlayerPrefs from localStorage (Unity WebGL uses this)
      var keysToRemove = [];
      for (var i = 0; i < localStorage.length; i++) {
        var key = localStorage.key(i);
        // Unity PlayerPrefs keys typically start with the company/product name
        if (key && (key.indexOf('unity') !== -1 ||
                    key.indexOf('Unity') !== -1 ||
                    key.indexOf('Keaton') !== -1 ||
                    key.indexOf('Mental') !== -1 ||
                    key.indexOf('MentalBreak') !== -1)) {
          keysToRemove.push(key);
        }
      }
      keysToRemove.forEach(function(key) {
        localStorage.removeItem(key);
        cleared.push('localStorage:' + key);
      });

      // Clear IndexedDB (Unity WebGL PlayerPrefs backend)
      if (window.indexedDB) {
        // List known Unity IndexedDB databases
        var dbNames = [
          '/idbfs',
          'UnityCache',
          '/idbfs-test'
        ];

        var deletePromises = dbNames.map(function(dbName) {
          return new Promise(function(res) {
            try {
              var req = indexedDB.deleteDatabase(dbName);
              req.onsuccess = function() {
                cleared.push('indexedDB:' + dbName);
                res();
              };
              req.onerror = function() { res(); };
              req.onblocked = function() { res(); };
            } catch (e) {
              res();
            }
          });
        });

        Promise.all(deletePromises).then(function() {
          console.log('[CacheBuster] Cleared:', cleared);
          resolve(cleared);
        });
      } else {
        console.log('[CacheBuster] Cleared:', cleared);
        resolve(cleared);
      }
    });
  }

  /**
   * Force a hard reload bypassing cache
   */
  function hardReload() {
    sessionStorage.setItem(RELOAD_FLAG, '1');

    // Use cache-busting reload
    if (window.location.reload) {
      // Modern approach - add cache buster to URL
      var url = new URL(window.location.href);
      url.searchParams.set('_cb', Date.now());
      window.location.replace(url.toString());
    }
  }

  /**
   * Main version check
   */
  function checkVersion() {
    // Add timestamp to bypass CDN cache
    var url = VERSION_URL + '?_t=' + Date.now();

    fetch(url, {
      method: 'GET',
      cache: 'no-store',
      headers: {
        'Cache-Control': 'no-cache, no-store, must-revalidate',
        'Pragma': 'no-cache'
      }
    })
    .then(function(response) {
      if (!response.ok) {
        throw new Error('Version fetch failed: ' + response.status);
      }
      return response.json();
    })
    .then(function(data) {
      var serverVersion = data.version + '-' + data.buildNumber;
      var localVersion = localStorage.getItem(STORAGE_KEY);

      console.log('[CacheBuster] Server version:', serverVersion);
      console.log('[CacheBuster] Local version:', localVersion);

      if (localVersion && localVersion !== serverVersion) {
        console.log('[CacheBuster] Version mismatch! Clearing cache and reloading...');

        clearUnityCache().then(function() {
          // Store the new version BEFORE reloading
          localStorage.setItem(STORAGE_KEY, serverVersion);
          hardReload();
        });
      } else if (!localVersion) {
        // First visit - clear cache in case user has old data from before cache-buster existed
        console.log('[CacheBuster] First visit, clearing old cache and storing version');
        clearUnityCache().then(function() {
          localStorage.setItem(STORAGE_KEY, serverVersion);
        });
      } else {
        console.log('[CacheBuster] Version matches, no reload needed');
      }
    })
    .catch(function(error) {
      // Don't block the game if version check fails
      console.warn('[CacheBuster] Version check failed:', error);
    });
  }

  // Run check
  checkVersion();
})();
