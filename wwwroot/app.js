window.copyToClipboard = (text) => {
    navigator.clipboard.writeText(text);
}

window.scrollToBottom = (elementId) => {
    try {
        const element = document.getElementById(elementId)
        if (!element) return
        const lastChild = element.children[element.children.length - 1]
        lastChild.scrollIntoView()
    } catch (ex) {
        console.log(ex)
    }
}

document.addEventListener('keyup', (event) => {
    if (event.key === 'Enter' && event.target.classList.contains('mud-input-root-adorned-end')) {
        const input = event.target;
        const button = input.nextElementSibling?.nextElementSibling?.querySelector('button');
        if (button) {
            event.preventDefault();
            button.focus()
            button.click()
        }
    }
});
