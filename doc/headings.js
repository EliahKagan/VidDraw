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

    // FIXME: Only this still uses jQuery. Rewrite it and drop the dependency.
    function associateHeadingsWithSections() {
        $('section>h1, section>h2, section>h3, section>h4').each(function () {
            this.dataset.anchorId = this.parentElement.id;
        });
    }

    function addAnchorLinksToHeadings() {
        const anchors = new AnchorJS({placement: 'right'});

        anchors.options.titleText = 'Link to this section';
        anchors.add('section>h2');

        anchors.options.titleText = 'Link to this subsection';
        anchors.add('section>h3')

        anchors.options.titleText = 'Link here';
        anchors.add('section>h4');
    }

    associateHeadingsWithSections();
    addAnchorLinksToHeadings();
})();
