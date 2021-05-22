// Copyright (c) 2021 Eliah Kagan
//
// Permission to use, copy, modify, and/or distribute this software for any
// purpose with or without fee is hereby granted.
//
// THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
// WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
// MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY
// SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
// WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION
// OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN
// CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.

(function () {
    'use strict';

    function getPxPerRem() {
        // Inspired by https://stackoverflow.com/a/42769683 on Stack Overflow
        // (by https://stackoverflow.com/users/806286/etham).
        const style = window.getComputedStyle(document.documentElement);
        return parseFloat(style.fontSize);
    }

    const windowOffsetBias = getPxPerRem() * 6;

    const level1Section = document.querySelector('section.level1');

    const allLevel2SectionsReversed = (function () {
        const sections = document.querySelectorAll('section.level2');
        return Object.freeze(Array.prototype.slice.call(sections).reverse());
    })();

    const navlinkSectionPairs = (function () {
        const navlinks = document.querySelectorAll('nav>ol>li>a');
        const pairs = Array.prototype.map.call(navlinks, function (link) {
            const id = link.getAttribute('href').slice(1);
            return [link, document.getElementById(id)];
        });
        return Object.freeze(pairs);
    })();

    function getCurrentMajorSection() {
        const offset = window.pageYOffset + windowOffsetBias;

        // IE doesn't have Array.prototype.find and also apparently doesn't
        // support for..of here. So use indexing.
        for (let i = 0; i < allLevel2SectionsReversed.length; ++i) {
            if (allLevel2SectionsReversed[i].offsetTop < offset) {
                return allLevel2SectionsReversed[i];
            }
        }

        return level1Section;
    }

    function updateActiveNavlink() {
        const currentSection = getCurrentMajorSection();

        navlinkSectionPairs.forEach(function (pair) {
            // IE apparently doesn't support unpacking here. So do it manually.
            const navlink = pair[0];
            const section = pair[1];

            if (section === currentSection) {
                navlink.classList.add('active');
            } else {
                navlink.classList.remove('active');
            }
        });
    }

    addEventListener('scroll', updateActiveNavlink);
    updateActiveNavlink();
})();
