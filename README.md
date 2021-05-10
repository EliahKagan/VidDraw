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
(It started out as a prototype for the video recording feature of a larger
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
5](https://dotnet.microsoft.com/download/dotnet/5.0) SDK. Then, to clone this
repository and build and run VidDraw, run:

```powershell
git clone https://github.com/EliahKagan/VidDraw.git
cd VidDraw
dotnet run
```

The first time you run
[`dotnet run`](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-run),
VidDraw will be built. Dependencies not included in this repository will be
[downloaded automatically](https://www.nuget.org/). If you want to build it
without running it, use
[`dotnet build`](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-build)
instead of `dotnet run`. (This creates a &ldquo;debug&rdquo; build. If you want
a &ldquo;release&rdquo; build instead, use `dotnet run -c Release` or `dotnet
build -c Release`.)

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

#### Post-process the video with FFmpeg

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

VidDraw&rsquo;s interface is clean and simple, yet remarkably confusing to use.
Sorry. Maybe some of the following information will help.

### Drawing and Recording

**Draw** as you would in any raster graphics editor.

When the mouse cursor is over the canvas and you press the primary mouse
button, recording begins. Moving the mouse while the primary mouse button is
pressed draws on the canvas. Recording continues until your release the button.

Even pressing the primary mouse button for a very short time records a video. A
single short click is sufficient, though the video may consist of only one
frame.

You will know VidDraw is recording because the border around the canvas turns
bright red. As long as it is red, VidDraw is recording. When it turns back to
light gray, recording has stopped and the video file is fully saved.

**To lift the pen while continuing to record**, press and hold another mouse
button. So long as at least one button is pressed, recording continues. This
lets you make individual videos of drawing sketches that consist of multiple,
separate curves.

If you ever want to continue recording while no mouse buttons are pressed,
place the mouse cursor on the canvas, press and hold any mouse button, draw the
cursor out of the canvas, press any other mouse button, and release the button
you first pressed. This effect, of keeping recording on even though
you&rsquo;re not holding down any mouse button, persists until your next
interaction with the canvas. So to stop recording, click the canvas. (If you
don&rsquo;t want to draw anything, click it with some button other than the
primary mouse button.) Or you can continue drawing, and the next time you
release all mouse buttons, recording will stop. Quitting VidDraw also ensures
that recording stops and the file is fully saved.

The pen color can be changed using the color picker [available in the
menu](#The-Menu).

The canvas in VidDraw is always 800&times;600.

### Save Location and Toast Notifications

VidDraw saves files in your Videos folder. The location of this folder is
configurable per-user in Windows, but by default it is the folder called
`Videos` and located directly in your home folder.

When a recording ends (while VidDraw is still running), VidDraw raises a [toast
notification](https://en.wikipedia.org/wiki/Pop-up_notification) informing you
of the filename as well as the encoding that was used for the video. Clicking
this notification opens a file browser (i.e., Explorer) window for the
destination folder, with the newly finished video selected.

VidDraw names files by the date and time at which it *started* recording them.
The filenames consist of `VidDraw capture` followed by the date and time, like
`VidDraw capture 2021-05-09 21-49-12`. (The ugly hyphen-delimited time is
because Windows filesystems like NTFS don&rsquo;t support colon characters in
filenames.) These video files exist immediately once recording has started,
though it may not be playable&mdash;and is not guaranteed to be openable by
other applications&mdash;until recording is completed.

Since clicking the notification opens the destination folder and selects the
file, you can rename it easily to whatever name you actually want it to have.

Quitting VidDraw clears any notifications that have not been clicked or
dismissed. *Unless* you were running multiple instances of VidDraw&mdash;in
that case, they all stick around until the last instance is closed. *Unless*
some instances are running in 64-bit mode while others are running in 32-bit
mode&mdash;in that case, quitting the last 64-bit instance clears any remaining
notifications from 64-bit instances, while quitting the last 32-bit instance
clears any remaining notifications from 32-bit instances.

VidDraw is only able to raise toast notification on Windows 10, version
10.0.17763 or later. If VidDraw can&rsquo;t raise toast notifications on your
system, it falls back to opening the Explorer window immediately when recording
finishes. But if VidDraw can&rsquo;t show you a notification because
you&rsquo;ve configured notifications in Windows 10 not to show them (or turned
them off), then VidDraw does *not* try to circumvent this by showing a file
browser window. In that case, nothing special happens when recording finishes.

You can always open the destination folder by clicking &ldquo;Open Videos
Folder&rdquo; [in VidDraw&rsquo;s menu](#The-Menu).

## The Menu

VidDraw augments its *system menu*&mdash;the application menu that you can open
by left- or right-clicking on the left side of the title bar, right-clicking
anywhere on the title bar, or pressing <kbd>Alt</kbd>+<kbd>Spacebar</kbd>, and
that contains standard items like &ldquo;Move&rdquo; and
&ldquo;Close&rdquo;&mdash;with encoding choices and some other operations
specific to VidDraw.

### Encoding (&ldquo;Codec&rdquo;) Choices

There are currently four video encodings available. The selected encoding has a
check mark to the left of it in the menu. Exactly one is selected at any given
time.

Please note that this doesn&rsquo;t affect the file format, i.e., container
filetype, which is always AVI. Rather, it affects the encoding of the video
stream.

#### Raw (frame copy)

This is the worst possible encoding and I don&rsquo;t recommend using it. Each
bitmap is captured from the canvas as-is, including the alpha channel data (the
&ldquo;A&rdquo; in &ldquo;ARGB&rdquo;). The canvas, and the recorded video
stream, use different conventions for the order in which each line (row of
pixels) is stored, so this order is reversed; otherwise, it is a raw
&ldquo;frame copy.&rdquo;

The only reason I put this in VidDraw was to demonstrate how to write video
frames in SharpAvi without an
[encoder](https://github.com/baSSiLL/SharpAvi/wiki/Using-Video-Encoders). But
you should use an encoder. If you want uncompressed video, I recommend using
[Uncompressed](#Uncompressed) instead, which leaves out the unused alpha
channel.

#### Uncompressed

If you want uncompressed video (where each frame is a bitmap), you should use this. It uses SharpAvi&rsquo;s `UncompressedVideoEncoder`. As [the SharpAvi wiki](https://github.com/baSSiLL/SharpAvi/wiki/Using-Video-Encoders#creating-video-encoder) says:

> The simplest [encoder] is
[UncompressedVideoEncoder](https://github.com/baSSiLL/SharpAvi/blob/master/SharpAvi/Codecs/UncompressedVideoEncoder.cs).
It does no real encoding, just flips image vertically and converts BGR32 data
to BGR24 data to reduce the size.

Video files created this way are quite large, if you&rsquo;re recording for
more a few seconds. If you&rsquo;re keeping videos you create this way, you may
want to encode them with a compressed codec afterwards. If your hard disk is
slow, you may experience lag while encoding this way. It&rsquo;s still better
than [Raw (frame copy)](#Raw), though.

#### Motion JPEG

[Motion JPEG](https://en.wikipedia.org/wiki/Motion_JPEG) is VidDraw&rsquo;s
default encoding. Each frame is converted to and stored as a JPEG image. This
uses SharpAvi&rsquo;s `MotionJpegVideoEncoderWpf`. From [the SharpAvi
wiki](https://github.com/baSSiLL/SharpAvi/wiki/Using-Video-Encoders#creating-video-encoder):

> Next is
[MotionJpegVideoEncoderWpf](https://github.com/baSSiLL/SharpAvi/blob/master/SharpAvi/Codecs/MotionJpegVideoEncoderWpf.cs)
which does Motion JPEG encoding. It uses
`System.Windows.Media.Imaging.JpegBitmapEncoder` under the hood. Besides
dimensions, you provide the desired quality level to its constructor, ranging
from 1 (low quality, small size) to 100 (high quality, large size).

VidDraw uses a quality of 100, since the pen in VidDraw makes 1-pixel-thick
curves, and lower qualities of Motion JPEG (like the oft-used 70) tend not to
render such curves crisply.

The file size is unhappily large, but nowhere near as bad as the uncompressed encodings.

#### H.264 (MPEG-4 AVC)



## Known Bugs

### Sometimes there is an initial lag on the first recording.

This seems to happen mainly while debugging&mdash;even a debug build that is
being run (via `dotnet run` or by directly running the compiled executable) is
most often free of it. But it seems to happen occasionally even outside of
debugging.

This is annoying because, when it happens, it usually results in a straight
line segment appearing on the canvas (and in the video) that the user
didn&rsquo;t intend to draw.

VidDraw could do some operations asynchronously that it does on the UI thread.
Most or all file I/O could, and should, be asynchronous. Implementing this will
require thought about what to do in some race conditions that cannot currently
happen. I don&rsquo;t know if that would be sufficient to fix this bug.

### Video files are not always playable on all players.

[As detailed above](#A-problem-with-H.264-in-VidDraw), when H.264 encoding is
selected and x264vfw has not been configured to make every frame a keyframe
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
