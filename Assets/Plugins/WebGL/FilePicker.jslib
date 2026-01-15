mergeInto(LibraryManager.library, {
  OpenFileDialog: function(gameObjectNamePtr, callbackMethodPtr) {
    console.log('[FilePicker] OpenFileDialog called');
    try {
      var gameObjectName = UTF8ToString(gameObjectNamePtr);
      var callbackMethod = UTF8ToString(callbackMethodPtr);
      console.log('[FilePicker] target:', gameObjectName, 'callback:', callbackMethod);

      var input = document.createElement('input');
      input.type = 'file';
      input.accept = '.json,application/json';

      // Safari-friendly: keep it in DOM, not off-screen, not fully invisible, clickable
      input.style.position = 'fixed';
      input.style.left = '10px';
      input.style.top = '10px';
      input.style.width = '1px';
      input.style.height = '1px';
      input.style.opacity = '0.01';
      input.style.zIndex = '999999';

      document.body.appendChild(input);
      console.log('[FilePicker] input appended to DOM');

      // Extra debug: listen to low-level DOM events to see if browser treats click as a user gesture
      input.addEventListener('click', function() { console.log('[FilePicker] input DOM click event fired'); });
      input.addEventListener('focus', function() { console.log('[FilePicker] input focus event fired'); });
      input.addEventListener('blur', function() { console.log('[FilePicker] input blur event fired'); });

      // Listen to window focus/blur to detect when file dialog steals focus
      var _windowFocusHandler = function() { console.log('[FilePicker] window focus regained (possible file dialog closed)'); };
      var _windowBlurHandler = function() { console.log('[FilePicker] window lost focus (possible file dialog opened)'); };
      window.addEventListener('focus', _windowFocusHandler);
      window.addEventListener('blur', _windowBlurHandler);

      // Create a small visible manual button as fallback so user can click directly
      var overlay = document.createElement('div');
      var btn = document.createElement('button');
      btn.textContent = 'Dosya Seç';
      // minimal visible styling so it can be clicked during tests
      overlay.style.position = 'fixed';
      overlay.style.right = '12px';
      overlay.style.top = '12px';
      overlay.style.zIndex = '1000001';
      overlay.style.background = 'rgba(0,0,0,0.5)';
      overlay.style.padding = '6px';
      overlay.style.borderRadius = '6px';
      btn.style.fontSize = '12px';
      btn.style.color = '#fff';
      btn.style.background = '#007acc';
      btn.style.border = 'none';
      btn.style.padding = '6px 8px';
      btn.style.cursor = 'pointer';
      overlay.appendChild(btn);
      document.body.appendChild(overlay);
      console.log('[FilePicker] manual overlay button added (visible)');

      var _removeOverlay = function() {
        try { if (overlay && overlay.parentNode) overlay.parentNode.removeChild(overlay); } catch(e){}
      };

      btn.addEventListener('click', function(ev) {
        try {
          console.log('[FilePicker] manual button clicked');
          // trigger actual file input click
          input.click();
          // hide overlay while dialog is open
          overlay.style.display = 'none';
        } catch (e) {
          console.error('[FilePicker] error on manual button click', e);
        }
      });

      input.onchange = function() {
        console.log('[FilePicker] input.onchange fired');
        try {
          if (!input.files || input.files.length === 0) {
            console.log('[FilePicker] no files selected');
            if (typeof SendMessage !== 'undefined') {
              console.log('[FilePicker] sending empty result via SendMessage');
              SendMessage(gameObjectName, callbackMethod, "");
            } else {
              console.warn('[FilePicker] SendMessage is undefined');
            }
            document.body.removeChild(input);
            return;
          }

          var file = input.files[0];
          console.log('[FilePicker] file selected:', file.name, file.size);
          var reader = new FileReader();

          reader.onload = function() {
            try {
              var text = reader.result || "";
              console.log('[FilePicker] reader.onload, length:', text.length);
              if (typeof SendMessage !== 'undefined') {
                SendMessage(gameObjectName, callbackMethod, text);
                console.log('[FilePicker] SendMessage called with file contents');
              } else {
                console.warn('[FilePicker] SendMessage is undefined on onload');
              }
            } catch (e) {
              console.error('[FilePicker] error in reader.onload handler', e);
            } finally {
              if (input && input.parentNode) document.body.removeChild(input);
              window.removeEventListener('focus', _windowFocusHandler);
              window.removeEventListener('blur', _windowBlurHandler);
              try { if (overlay && overlay.parentNode) overlay.parentNode.removeChild(overlay); } catch(e){}
            }
          };

          reader.onerror = function(ev) {
            console.error('[FilePicker] reader.onerror', ev);
            if (typeof SendMessage !== 'undefined') SendMessage(gameObjectName, callbackMethod, "");
            if (input && input.parentNode) document.body.removeChild(input);
            window.removeEventListener('focus', _windowFocusHandler);
            window.removeEventListener('blur', _windowBlurHandler);
            try { if (overlay && overlay.parentNode) overlay.parentNode.removeChild(overlay); } catch(e){}
          };

          reader.readAsText(file);
          console.log('[FilePicker] reader.readAsText called');
        } catch (e) {
          console.error('[FilePicker] error inside onchange handler', e);
          if (typeof SendMessage !== 'undefined') SendMessage(gameObjectName, callbackMethod, "");
          if (input && input.parentNode) document.body.removeChild(input);
          window.removeEventListener('focus', _windowFocusHandler);
          window.removeEventListener('blur', _windowBlurHandler);
          try { if (overlay && overlay.parentNode) overlay.parentNode.removeChild(overlay); } catch(e){}
        }
      };

      setTimeout(function() {
        try {
          input.focus();
          input.click();
          console.log('[FilePicker] input.clicked');
        } catch (e) {
          console.error('[FilePicker] error during click', e);
        }
      }, 0);
    } catch (e) {
      console.error('[FilePicker] unexpected error', e);
      try {
        var gameObjectNameFallback = (typeof UTF8ToString === 'function' && gameObjectNamePtr) ? UTF8ToString(gameObjectNamePtr) : '';
        var callbackMethodFallback = (typeof UTF8ToString === 'function' && callbackMethodPtr) ? UTF8ToString(callbackMethodPtr) : '';
        if (typeof SendMessage !== 'undefined') SendMessage(gameObjectNameFallback, callbackMethodFallback, "");
      } catch (ee) {
        console.error('[FilePicker] error while attempting fallback SendMessage', ee);
      }
    }
  }
});
