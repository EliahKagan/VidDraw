<!--
  Copyright (c) 2021 Eliah Kagan

  Permission to use, copy, modify, and/or distribute this software for any
  purpose with or without fee is hereby granted.

  THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH
  REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY
  AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT,
  INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM
  LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR
  OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR
  PERFORMANCE OF THIS SOFTWARE.
-->

# VidDraw - record video as you draw

*Written in 2021 by Eliah Kagan &lt;degeneracypressure@gmail.com&gt;*.

**VidDraw** is a Windows program that records a video of a canvas as you draw
on it. Recording starts when you begin drawing and stops when you&rsquo;re no
longer holding down any mouse buttons.

The main purpose of VidDraw is to demonstrate usage of
[SharpAvi]([SharpAvi](https://github.com/baSSiLL/SharpAvi)), which wraps
[VfW](https://docs.microsoft.com/en-us/windows/win32/api/vfw/), and [toast
notifications](https://docs.microsoft.com/en-us/windows/uwp/design/shell/tiles-and-notifications/send-local-toast?tabs=uwp).
(It started out as a prototype for the videorecording feature of a larger
program I&rsquo;ve been working on.) But it is also kind of a fun toy.

This is VidDraw alpha 1. It still has [some usability bugs](#Known-Bugs).

## License

VidDraw is [free software](https://en.wikipedia.org/wiki/Free_software). The
program [is itself licensed](LICENSE-0BSD.txt) under
[0BSD](https://spdx.org/licenses/0BSD.html) (&ldquo;Zero-Clause BSD
License,&rdquo; also known as the [Free Public License
1.0.0](https://opensource.org/licenses/0BSD)), which is a [&ldquo;public domain
equivalent&rdquo;](https://en.wikipedia.org/wiki/Public-domain-equivalent_license)
license. VidDraw&rsquo;s dependencies are also free, but they are offered under
other licenses. See [Dependencies](#Dependencies) below for details.

Since some of VidDraw&rsquo;s dependencies are included directly in this
repository, some of the files in this repository were not written by me and are
licensed under terms other than 0BSD. See [`LICENSE.md`](LICENSE.md) for full
details.

## Setup

This alpha version of VidDraw doesn&rsquo;t have binary downloads. To run
VidDraw, make sure you have the [.NET
5](https://dotnet.microsoft.com/download/dotnet/5.0) SDK, then clone this
repository and build VidDraw by running:

```powershell
git clone https://github.com/EliahKagan/VidDraw.git
cd VidDraw
dotnet run
```

The first time you run [`dotnet
run`](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-run), VidDraw
will be built. Dependencies not included in this repository will be [downloaded
automatically](https://www.nuget.org/). If you want to build it without running
it, use
[`dotnet build`](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-build)
instead of `dotnet run`. (This creates a &ldquo;debug&rdquo; build. If you want
a &ldquo;release&rdquo; build instead, use `dotnet run -c Release` or
`dotnet build -c Release`.)

That&rsquo;s all you need. If you want to read about codec stuff&hellip; read
on! Otherwise, you may want to skip to [Usage Tips](#Usage-Tips), or just try
out the program.

### Optional: Install and configure x264vfw to get H.264 support

If you want VidDraw to be able to encode H.264 video, you&rsquo;ll need
[x264vfw](https://sourceforge.net/projects/x264vfw/). You don&rsquo;t have to
install that before VidDraw, though. You can even install it while VidDraw is
already running, and H.264 will be ungrayed in VidDraw&rsquo;s menu.

In the x264vfw configuration, you should check the box for &ldquo;Zero
Latency&rdquo; [to avoid picture
freezes](https://github.com/baSSiLL/SharpAvi/wiki/Using-Video-Encoders#creating-video-encoder)
while encoding video in real time. Note that the 32-bit and 64-bit x264vfw
codecs are configured separately; changing the configuration for one
doesn&rsquo;t affect the other.

x264fw&rsquo;s installer creates &ldquo;Configure x264vfw&rsquo; and
&ldquo;Configure x264vfw64&rdquo; shortcuts (or just one, if you told it to
install the codec for just one architecture or you&rsquo;re on a 32-bit
system). Besides those shortcuts, another to open the x264vfw configuration
dialog is from the menu in VidDraw: so long as x264vfw is installed for the
same architecture as the VidDraw process, a &ldquo;Configure x264vfw
(x86)&rdquo; or &ldquo;Configure x264vfw (x64)&rdquo; menu item will appear.

### A problem with H.264 in VidDraw

Encoding with H.264 produces much smaller files than Motion JPEG, which is
VidDraw&rsquo;s default and itself much smaller than encodings without any
compression. But I&rsquo;ve found that these files&mdash;files recorded in real
time in the way VidDraw records them, *not* H.264 video in general&mdash;are
not playable on all players. [This may be due to a bug in VidDraw, but
I&rsquo;m not sure.](#Video-files-are-not-always-playable-on-all-players) The
excellent and very popular VLC does not play them. Windows Media Player and the
Movies & TV app (and also [MPC-HC](https://mpc-hc.org/)) do play them.

Other than just using Motion JPEG instead (or waiting until VidDraw is out of
alpha?), there are a couple workarounds to produce a video that VLC will play:

#### Configure x264vfw to make every frame a keyframe

At the cost of considerably larger files (but still not as big as Motion JPEG),
making every frame a [keyframe](https://en.wikipedia.org/wiki/Key_frame) fixes
the problem in VLC. To do that, open the x264vfw configuration dialog
and&mdash;in the textarea labeled &ldquo;Extra command line (for advanced
users)&rdquo;&mdash;put:

```text
--keyint 1
```

*See [x264vfw Compression - WTF am I doing
wrong?](https://forums.guru3d.com/threads/x264vfw-compression-wtf-am-i-doing-wrong.373036/)
on the [Guru3D.com](https://www.guru3d.com/) forums.*

#### Postprocess the video with FFmpeg

Given a video from VidDraw that VLC cannot open, [FFmpeg](https://ffmpeg.org/)
will repair it:

```powershell
ffmpeg -err_detect ignore_err -i "VidDraw capture 2021-05-08 16-38-04.avi" -c copy "VidDraw capture 2021-05-08 16-38-04 fixed.avi"
```

You would run that from the `Videos` folder (where VidDraw saved the file).
Change `VidDraw capture 2021-05-08 16-38-04.avi` to the actual name of the file
VidDraw created. The second filename names FFmpeg&rsquo;s output file; put
whatever you like for that.

*See [Gyan](https://video.stackexchange.com/users/1871/gyan)&rsquo;s
[answer](https://video.stackexchange.com/a/18226) to [Fix bad files and streams
with ffmpeg so VLC and other players would not
crash](https://video.stackexchange.com/questions/18220/fix-bad-files-and-streams-with-ffmpeg-so-vlc-and-other-players-would-not-crash)
on [Video Production Stack Exchange](https://video.stackexchange.com/).*

## Usage Tips



## Known Bugs

### Video files are not always playable on all players.

[As detailed above](#A-problem-with-H.264-in-VidDraw), when H.264 encoding is
selected and x264vfw has not been configured to make every frame a key frame
(which increases file size dramatically, albeit still less than the other
encodings), VLC cannot play the file. This is even though VLC fully supports
H.264 (and manages to play even most broken H.264 files). It may be a problem
with the way I&rsquo;ve configured x264vfw. But [FFmpeg](https://ffmpeg.org/)
is able to repair the files, and it reports problems with them, so I think this
is a bug.

I can produce this problem (on the same system) with the SharpAvi sample
application&mdash;both the [upstream
version](https://github.com/baSSiLL/SharpAvi/tree/master/Sample) targeting the
[upstream SharpAvi](https://www.nuget.org/packages/SharpAvi/) and [my forked
version](https://github.com/EliahKagan/SharpAvi/tree/net5/Sample) targeting
[the fork of SharpAvi](https://www.nuget.org/packages/SharpAvi.Net5) that
VidDraw uses&mdash;but it remains unclear to me if this is due to a bug in
SharpAvi, due to its sensitivity to x264vfw settings. I am also able to produce
the problem in the SharpAvi sample application with
[Xvid](https://www.xvid.com/) (but, likewise, this might turn out to be due to
how I have Xvid configured).

### Menu items are cumbersome to access while drawing.

Some operations that users are likely to want to do while
recording&mdash;especially opening the dialog box to change the pen
color&mdash;should have shortcut keys but do not. Some design choices have to
be made to implement this correctly: if the user presses a shortcut key to open
a modal dialog box, should recording automatically continue after the dialog
box exits?

### It might be useful to be able to pause.

VidDraw&rsquo;s user experience is intended to be simple and spontaneous, so I
haven&rsquo;t included any option to pause and unpause recording. If recording
*ever* pauses, then users are likely to assume, and some may prefer, that
opening a modal dialog box or changing focus to another application will pause
it. So, if pausing is added, options to enable (and disable) automatic pausing
in those situations should be added too.

Fixing this may help with [Menu items are cumbersome to access while
drawing](#Menu-items-are-cumbersome-to-access-while-drawing) as well.


### The pen thickness should be adjustable in some way.

Being able to vary the pen&rsquo;s size would make drawing more fun.

This could be done by holding down modifier keys (<kbd>Shift</kbd>,
<kbd>Ctrl</kbd>, <kbd>Alt</kbd>) and/or by selecting something in the menu.

### User experience on older Windows versions could be better.

Although it is not a goal for VidDraw to fully support any versions of Windows
older than 10.0.17763, it might be good to come up with some other behavior
than immediately opening the destination folder each time on systems that
don&rsquo;t support toast notifications.

Even older Windows systems, such as Windows 7, have very thin title bars, at
least when styling is turned off or classic styling is used. This makes the
border around the canvas (which turns red to indicate recording) look
excessively thick. It&rsquo;s probably not worth it to detect and handle this
situation, but perhaps there&rsquo;s some simple way.
