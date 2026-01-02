// WebAudioUnlocker.jslib
// JavaScript plugin to unlock Web Audio context for Unity WebGL builds
// Fixes browser autoplay restrictions that block audio from keyboard input

mergeInto(LibraryManager.library, {
    WebAudio_TryResume: function() {
        console.log('[WebAudioUnlocker] TryResume called');

        // Get FMOD's AudioContext (Unity WebGL uses FMOD's context)
        var ctx = null;

        // Try to find FMOD's AudioContext first (most reliable)
        if (typeof FMOD !== 'undefined' && FMOD.audioContext) {
            console.log('[WebAudioUnlocker] Found FMOD.audioContext');
            ctx = FMOD.audioContext;
        }
        // Try Unity's global audioContext variable
        else if (typeof WEBAudio !== 'undefined' && WEBAudio.audioContext) {
            console.log('[WebAudioUnlocker] Found WEBAudio.audioContext');
            ctx = WEBAudio.audioContext;
        }
        // Try Module.SDL2 audio context (older Unity versions)
        else if (typeof Module !== 'undefined' && Module.SDL2 && Module.SDL2.audioContext) {
            console.log('[WebAudioUnlocker] Found Module.SDL2.audioContext');
            ctx = Module.SDL2.audioContext;
        }

        if (!ctx) {
            console.warn('[WebAudioUnlocker] No AudioContext found - FMOD may not be initialized yet');
            return 0; // Failed
        }

        console.log('[WebAudioUnlocker] AudioContext state:', ctx.state);

        if (ctx.state === 'suspended') {
            console.log('[WebAudioUnlocker] AudioContext suspended, attempting resume...');
            ctx.resume().then(function() {
                console.log('[WebAudioUnlocker] AudioContext resumed successfully');
            }).catch(function(err) {
                console.error('[WebAudioUnlocker] Failed to resume:', err);
            });
            return 1; // Attempted
        } else if (ctx.state === 'running') {
            console.log('[WebAudioUnlocker] AudioContext already running');
            return 2; // Already running
        }

        console.warn('[WebAudioUnlocker] AudioContext in unknown state:', ctx.state);
        return 0; // Unknown state
    },

    WebAudio_GetState: function() {
        // Returns: 0=unknown, 1=suspended, 2=running, 3=closed
        var ctx = null;

        // Try to find FMOD's AudioContext first
        if (typeof FMOD !== 'undefined' && FMOD.audioContext) {
            ctx = FMOD.audioContext;
        }
        // Try Unity's global audioContext variable
        else if (typeof WEBAudio !== 'undefined' && WEBAudio.audioContext) {
            ctx = WEBAudio.audioContext;
        }
        // Try Module.SDL2 audio context
        else if (typeof Module !== 'undefined' && Module.SDL2 && Module.SDL2.audioContext) {
            ctx = Module.SDL2.audioContext;
        }

        if (!ctx) return 0;

        if (ctx.state === 'suspended') return 1;
        if (ctx.state === 'running') return 2;
        if (ctx.state === 'closed') return 3;
        return 0;
    }
});
