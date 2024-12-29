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

const findClosestParentWithClass = (element, className) => {
    let currentElement = element;
    while (currentElement) {
        if (currentElement.classList && currentElement.classList.contains(className)) {
            return currentElement; // Found the parent
        }
        currentElement = currentElement.parentElement;
    }
    return null; // No parent found with the class
};


window.setEnterToSend = (className) => {
    try {
        document.addEventListener('keyup', (event) => {
            if (event.key === 'Enter') {
                const input = event.target;
                const parentWithClass = findClosestParentWithClass(input, className);

                if (parentWithClass) {
                    const nextSibling = input.nextElementSibling;
                    if (nextSibling) {
                        event.preventDefault();
                        const button = nextSibling.querySelector('button')
                        if (button) {
                            button.focus()
                            button.click()
                        }
                    }
                }
            }
        });

    } catch (ex) {
        console.log(ex);
    }
};

function calculateFillMiddleMinHeight() {
    const fillMiddleElements = document.querySelectorAll('.fill-middle');
    const instructionPanel = document.querySelector('.instruction-panel');

    if (!fillMiddleElements.length || !instructionPanel) {
        return;
    }

    fillMiddleElements.forEach(element => {
        const windowHeight = window.innerHeight;
        const elementTop = element.offsetTop;
        const instructionPanelHeight = instructionPanel.offsetHeight;
        const calculatedMinHeight = windowHeight - elementTop - instructionPanelHeight - 48;
        element.style.minHeight = `${calculatedMinHeight}px`;
    });
}

document.addEventListener('DOMContentLoaded', function () { setTimeout(calculateFillMiddleMinHeight, 100) });
window.addEventListener('resize', calculateFillMiddleMinHeight);