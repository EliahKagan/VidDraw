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

-- Returns the given string but without any trailing whitespace.
local function trim_right(text)
  return text:gsub('%s+$', '')
end

-- Reads the contents of a text file at the given path and formats it as an
-- HTML block comment with an inner indent specified by INDENT.
local function format_block_comment(path)
  local margin = string.rep(' ', INDENT)

  local out = {'<!--'}
  for line in io.lines(path) do
    out[#out + 1] = trim_right(margin .. line)
  end
  out[#out + 1] = '-->'

  return trim_right(table.concat(out, '\n')) .. '\n'
end

-- VidDraw's 0BSD license text, presented as an HTML block comment.
local COMMENTED_0BSD = format_block_comment('../COPYING.0BSD')

-- Detects block comments with exactly the text of the accompanying license.
-- Such text is redundant, as the template file puts the license at the top.
local function is_license_comment(el)
  assert(el.t == 'RawBlock',
         "Can't search for license comment outside of RawBlock")

  return el.format == 'html' and el.text == COMMENTED_0BSD
end

-- Detects raw HTML blocks that begin with the opening or closing tag of a
-- details element, with no attributes and no excess whitespace. Under the
-- convention adhered to in README.md, this is always either an opening tag
-- followed by a complete summary element, or a closing tag by itself.
local function is_details_markup(el)
  assert(el.t == 'RawBlock',
         "Can't search for details markup outside of RawBlock")

  return el.format == 'html' and el.text:find('^</?details>')
end

-- Erase license comments and introductions and closings of details elements.
function RawBlock(el)
  if is_license_comment(el) or is_details_markup(el) then
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

local upward_traversals = {}

local function note_upward_traversal(target)
  if not upward_traversals[target] then
    upward_traversals[target] = true
    print('Upward traversal: ' .. target)
  end
end

-- Adjust a property (whose value consists of a path) to account for different
-- input and output directories.
local function adjust_path(el, property)
  assert(not (el[property]:find('^/') or el[property]:find('^file:')),
         "Can't adjust absolute local href")

  -- hrefs to an id, or that point to a full URL, need no adjustment.
  if el[property]:find('^#') or el[property]:find('^%w+:') then
    return
  end

  -- We're in doc, so if the href points there, remove that path prefix.
  local count
  el[property], count = el[property]:gsub('^doc/', '')

  -- But if the href doesn't point in doc, add a parent path prefix.
  if count == 0 then
    note_upward_traversal(el[property])
    el[property] = '../' .. el[property]
  end
end

-- If the link doesn't already have title text, use its address.
local function add_missing_title_text(el)
  if el.title == '' then
    el.title = el.target
  end
end

-- Adjust links' relative paths and add title text if missing.
function Link(el)
  adjust_path(el, 'target')
  add_missing_title_text(el)
  return el
end

-- Adjust images' relative paths.
function Image(el)
  adjust_path(el, 'src')
  return el
end
