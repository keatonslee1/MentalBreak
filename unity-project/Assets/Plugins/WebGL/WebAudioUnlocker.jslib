// WebAudioUnlocker.jslib
// JavaScript plugin to unlock Web Audio context for Unity WebGL builds
// Fixes browser autoplay restrictions that block audio from keyboard input

mergeInto(LibraryManager.library, {
    WebAudio_TryResume: function() {
        // Get FMOD's AudioContext (Unity WebGL uses FMOD's context)
        var ctx = null;

        // Try to find FMOD's AudioContext first
        if (typeof FMOD !== 'undefined' && FMOD.audioContext) {
            ctx = FMOD.audioContext;
        }
        // Fallback to global AudioContext
        else if (typeof AudioContext !== 'undefined') {
            ctx = new (window.AudioContext || window.webkitAudioContext)();
        }
        else if (typeof window.AudioContext !== 'undefined') {
            ctx = new window.AudioContext();
        }
        else if (typeof window.webkitAudioContext !== 'undefined') {
            ctx = new window.webkitAudioContext();
        }

        if (!ctx) {
            console.warn('[WebAudioUnlocker] No AudioContext found');
            return 0; // Failed
        }

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

        return 0; // Unknown state
    },

    WebAudio_GetState: function() {
        // Returns: 0=unknown, 1=suspended, 2=running, 3=closed
        var ctx = null;

        if (typeof FMOD !== 'undefined' && FMOD.audioContext) {
            ctx = FMOD.audioContext;
        }

        if (!ctx) return 0;

        if (ctx.state === 'suspended') return 1;
        if (ctx.state === 'running') return 2;
        if (ctx.state === 'closed') return 3;
        return 0;
    }
});
