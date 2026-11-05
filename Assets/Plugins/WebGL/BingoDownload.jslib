mergeInto(LibraryManager.library, {
  BingoDownloadPng: function (pointer, length, filename) {
    try {
      // Copy before returning: Unity can release its managed byte array afterwards.
      var blob = new Blob([HEAPU8.slice(pointer, pointer + length)], { type: 'image/png' });
      var url = URL.createObjectURL(blob);
      var link = document.createElement('a');
      link.href = url;
      link.download = UTF8ToString(filename);
      link.style.display = 'none';
      document.body.appendChild(link);
      link.click();
      link.remove();
      setTimeout(function () { URL.revokeObjectURL(url); }, 60000);
      return 1;
    } catch (error) {
      console.error('Bingo PNG download failed:', error);
      return 0;
    }
  }
});
