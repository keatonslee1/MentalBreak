// WebAudioUnlocker.jslib
// JavaScript plugin to unlock Web Audio context for Unity WebGL builds
// Fixes browser autoplay restrictions that block audio from keyboard input

mergeInto(LibraryManager.library, {
    WebAudio_TryResume: function() {
        console.log('[WebAudioUnlocker] TryResume called');

        var ctx = null;

        // Method 1: Check FMOD global
        if (typeof FMOD !== 'undefined') {
            console.log('[WebAudioUnlocker] FMOD object exists');
            if (FMOD.audioContext) {
                console.log('[WebAudioUnlocker] Found FMOD.audioContext');
                ctx = FMOD.audioContext;
            }
        }

        // Method 2: Search window for existing AudioContext instances
        if (!ctx && typeof window.AudioContext !== 'undefined') {
            console.log('[WebAudioUnlocker] Searching window for AudioContext...');
            for (var key in window) {
                try {
                    if (window[key] && (window[key] instanceof AudioContext ||
                        (typeof window.webkitAudioContext !== 'undefined' && window[key] instanceof webkitAudioContext))) {
                        console.log('[WebAudioUnlocker] Found AudioContext at window.' + key);
                        ctx = window[key];
                        break;
                    }
                } catch(e) {
                    // Ignore errors from restricted properties
                }
            }
        }

        // Method 3: Check Unity's Module.SDL2
        if (!ctx && typeof Module !== 'undefined') {
            console.log('[WebAudioUnlocker] Checking Module...');
            if (Module.SDL2 && Module.SDL2.audioContext) {
                console.log('[WebAudioUnlocker] Found Module.SDL2.audioContext');
                ctx = Module.SDL2.audioContext;
            }
        }

        // Method 4: Check unityInstance global
        if (!ctx && typeof unityInstance !== 'undefined') {
            console.log('[WebAudioUnlocker] Checking unityInstance...');
            if (unityInstance.Module && unityInstance.Module.SDL2 && unityInstance.Module.SDL2.audioContext) {
                console.log('[WebAudioUnlocker] Found unityInstance.Module.SDL2.audioContext');
                ctx = unityInstance.Module.SDL2.audioContext;
            }
        }

        // Method 5: Check Unity canvas element
        if (!ctx) {
            console.log('[WebAudioUnlocker] Checking Unity canvas...');
            var canvas = document.querySelector('#unity-canvas') || document.querySelector('canvas');
            if (canvas && canvas.audioContext) {
                console.log('[WebAudioUnlocker] Found canvas.audioContext');
                ctx = canvas.audioContext;
            }
        }

        if (!ctx) {
            console.warn('[WebAudioUnlocker] Could not find AudioContext anywhere');
            // Log available audio-related globals for debugging
            var audioGlobals = [];
            for (var k in window) {
                if (k.toLowerCase().includes('audio') || k.toLowerCase().includes('fmod')) {
                    audioGlobals.push(k);
                }
            }
            console.log('[WebAudioUnlocker] Audio-related globals:', audioGlobals);
            return 0; // Failed
        }

        console.log('[WebAudioUnlocker] AudioContext found! State:', ctx.state);

        if (ctx.state === 'suspended') {
            console.log('[WebAudioUnlocker] Resuming suspended context...');
            ctx.resume().then(function() {
                console.log('[WebAudioUnlocker] Resume successful');
            }).catch(function(err) {
                console.error('[WebAudioUnlocker] Resume failed:', err);
            });
            return 1; // Attempted
        } else if (ctx.state === 'running') {
            console.log('[WebAudioUnlocker] Already running');
            return 2; // Already running
        }

        console.warn('[WebAudioUnlocker] AudioContext in unknown state:', ctx.state);
        return 0; // Unknown state
    },

    WebAudio_GetState: function() {
        // Returns: 0=unknown, 1=suspended, 2=running, 3=closed
        var ctx = null;

        // Method 1: Check FMOD global
        if (typeof FMOD !== 'undefined' && FMOD.audioContext) {
            ctx = FMOD.audioContext;
        }

        // Method 2: Search window for existing AudioContext instances
        if (!ctx && typeof window.AudioContext !== 'undefined') {
            for (var key in window) {
                try {
                    if (window[key] && (window[key] instanceof AudioContext ||
                        (typeof window.webkitAudioContext !== 'undefined' && window[key] instanceof webkitAudioContext))) {
                        ctx = window[key];
                        break;
                    }
                } catch(e) {
                    // Ignore errors from restricted properties
                }
            }
        }

        // Method 3: Check Unity's Module.SDL2
        if (!ctx && typeof Module !== 'undefined') {
            if (Module.SDL2 && Module.SDL2.audioContext) {
                ctx = Module.SDL2.audioContext;
            }
        }

        // Method 4: Check unityInstance global
        if (!ctx && typeof unityInstance !== 'undefined') {
            if (unityInstance.Module && unityInstance.Module.SDL2 && unityInstance.Module.SDL2.audioContext) {
                ctx = unityInstance.Module.SDL2.audioContext;
            }
        }

        // Method 5: Check Unity canvas element
        if (!ctx) {
            var canvas = document.querySelector('#unity-canvas') || document.querySelector('canvas');
            if (canvas && canvas.audioContext) {
                ctx = canvas.audioContext;
            }
        }

        if (!ctx) return 0;

        if (ctx.state === 'suspended') return 1;
        if (ctx.state === 'running') return 2;
        if (ctx.state === 'closed') return 3;
        return 0;
    }
});
