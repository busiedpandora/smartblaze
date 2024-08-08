window.highlightAllCodeBlocks = function() {
    document.querySelectorAll('pre code:not(.hljs)').forEach((block) => {
        hljs.highlightElement(block);
    });
}