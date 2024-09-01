window.highlightAllCodeBlocks = function() {
    document.querySelectorAll('pre code:not(.hljs)').forEach((block) => {
        hljs.highlightElement(block);
    });
}

window.highlightCodeBlock = function() {
    let code = document.querySelector('pre code');
    if(code) {
        hljs.highlightElement(code);
    }
}