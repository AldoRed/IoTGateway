function NativeHeader() {
    const header = document.getElementById("native-header");
    Array.from(header.children[0].children[1].getElementsByTagName("ul")).forEach(subMenu => {
        const sibling = subMenu.nextElementSibling || subMenu.previousElementSibling;

        // if link dose not go anywhere, expand on click
        sibling.addEventListener("click", event => {
            // expand menue on mobile view
            if (window.matchMedia("screen and (max-width: 900px)").matches) {
                event.preventDefault(); // if link, prevent opening
                subMenu.toggleAttribute("expanded");
            }
        })

        subMenu.parentElement.addEventListener("mouseenter", () => {
            // if on computer, make sure dropdowns expand beyond the screen

            const rect = subMenu.getBoundingClientRect();
            const rightX = rect.left + rect.width

            // content is overflowing
            if (rightX > window.innerWidth) {
                // go to top submenu
                let element = subMenu
                while(element.parentElement.parentElement.parentElement.tagName.toLowerCase() == "li")
                {
                    element = element.parentElement.parentElement
                }

                let oldOffset = 0
                if (element.style.transform)
                    oldOffset += Number(element.style.transform.split("(")[1].split("px")[0])
                element.style.transform = `translateX(${oldOffset + window.innerWidth - rightX}px)`

                const topLevelLi = element.parentElement

                const handler = () => {
                    topLevelLi.removeEventListener("mouseenter", handler);
                    element.style.transform = ""
                  };
                topLevelLi.addEventListener("mouseenter", handler);
            }
        })
    })

    function ToggleNav() {
        header.toggleAttribute("data-visible");
    }

    return {
        ToggleNav
    }
}

let nativeHeader;

window.addEventListener("load", () => {
    nativeHeader = NativeHeader();
})

