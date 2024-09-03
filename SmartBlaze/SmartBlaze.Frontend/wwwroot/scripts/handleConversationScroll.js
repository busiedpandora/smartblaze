window.scrollToBottom = function(element) {
    if(!element) {
        return;
    }

    element.scrollTop = element.scrollHeight;
}