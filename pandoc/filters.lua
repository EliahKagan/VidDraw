-- pandoc filters

-- Copyright (c) 2021 Eliah Kagan
--
-- Permission to use, copy, modify, and/or distribute this software for any
-- purpose with or without fee is hereby granted.
--
-- THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
-- WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
-- MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY
-- SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
-- WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION
-- OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN
-- CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.

local INDENT = 2

local function trim_right(text)
  return text:gsub('%s+$', '')
end

local function format_block_comment(path)
  local margin = string.rep(' ', INDENT)

  local out = {'<!--'}
  for line in io.lines(path) do
    out[#out + 1] = trim_right(margin .. line)
  end
  out[#out + 1] = '-->'

  return trim_right(table.concat(out, '\n')) .. '\n'
end

local COMMENTED_0BSD = format_block_comment('../COPYING.0BSD')

-- Remove block comments that are exactly the text of the accompanying license.
-- This is to avoid repetition. The template file puts the license at the top.
function RawBlock(el)
  if el.format == 'html' and el.text == COMMENTED_0BSD then
    return {}
  end
end

-- Squash consecutive hyphens in header ids to a single hyphen (except in h1).
--
-- Pandoc gfm and GitHub disagree on ids of headings with <br>. I'm working
-- around it by using this filter and putting a space after each such <br> tag.
-- This is not a general solution to that problem, but it works for README.md.
function Header(el)
  if el.level ~= 1 then
    el.identifier = el.identifier:gsub('%-+', '-')
    return el
  end
end

-- Drop "doc/" prefixes in links (index.html, unlike README.md, goes in doc).
function Link(el)
  el.target = el.target:gsub('^doc/', '')
  return el
end
