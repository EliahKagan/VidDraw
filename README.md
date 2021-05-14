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
[SharpAvi](https://github.com/baSSiLL/SharpAvi), which wraps
[VfW](https://docs.microsoft.com/en-us/windows/win32/api/vfw/), and [toast
notifications](https://docs.microsoft.com/en-us/windows/uwp/design/shell/tiles-and-notifications/send-local-toast?tabs=uwp).
(It started out as a prototype for the video recording feature of a larger
program I&rsquo;ve been working on.) But it is also kind of a fun toy.

This is **VidDraw alpha 1**. It still has [some usability bugs](#known-bugs).

## License

VidDraw is [free software](https://en.wikipedia.org/wiki/Free_software). The
program [is itself licensed](LICENSE-0BSD.txt) under
[0BSD](https://spdx.org/licenses/0BSD.html) (&ldquo;Zero-Clause BSD
License,&rdquo; also known as the [Free Public License
1.0.0](https://opensource.org/licenses/0BSD)), which is a [&ldquo;public domain
equivalent&rdquo;](https://en.wikipedia.org/wiki/Public-domain-equivalent_license)
license. VidDraw&rsquo;s dependencies are also free, but they are offered under
other licenses. See [Dependencies](#dependencies) below for details.

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
on! Otherwise, you may want to skip to [Usage Tips](#usage-tips) or [The Menu](#the-menu), or just try
out the program.

### Optional: Get x264vfw for H.264 support

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

x264fw&rsquo;s installer creates &ldquo;Configure x264vfw&rdquo; and
&ldquo;Configure x264vfw64&rdquo; shortcuts (or just one, if you told it to
install the codec for just one architecture or you&rsquo;re on a 32-bit
system). Besides those shortcuts, another way to open the x264vfw configuration
dialog is from the menu in VidDraw: so long as x264vfw is installed for the
same architecture as the running VidDraw process, a &ldquo;Configure x264vfw
(x86)&rdquo; or &ldquo;Configure x264vfw (x64)&rdquo; menu item will appear.

### A problem with H.264 in VidDraw

Encoding with H.264 produces much smaller files than Motion JPEG, which is
VidDraw&rsquo;s default and itself much smaller than encodings without any
compression. But I&rsquo;ve found that these files&mdash;files recorded in real
time in the way VidDraw records them, *not* H.264 video in general&mdash;are
not playable on all players. [This may be due to a bug in VidDraw, but
I&rsquo;m not sure.](#video-files-are-not-always-playable-on-all-players) The
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

VidDraw&rsquo;s interface is clean and simple&hellip; yet remarkably confusing
to use. Sorry. Maybe some of the following information will help.

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

The pen color can be changed using [the color picker](#pick-color), available
in [the menu](#the-menu).

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
Folder&rdquo; [in VidDraw&rsquo;s menu](#the-menu).

## The Menu

VidDraw augments its *system menu*&mdash;the application menu that you can open
by left- or right-clicking on the left side of the title bar, right-clicking
anywhere on the title bar, or pressing <kbd>Alt</kbd>+<kbd>Space</kbd>, and
that contains standard items like &ldquo;Move&rdquo; and
&ldquo;Close&rdquo;&mdash;with [encoding choices](#encoding-codec-choices) and
some [other operations](#other-stuff-in-the-menu) specific to VidDraw.

Some menu items change settings. These changes are saved in a `VidDraw`
subdirectory of your per-user application data (`%APPDATA%`) directory, which
is usually the `%USERPROFILE%\AppData\Roaming` (where `%USERPROFILE%` is your
home directory).

### Encoding (&ldquo;Codec&rdquo;) Choices

There are currently four video encodings available. The selected encoding has a
check mark to the left of it in the menu. Exactly one is selected at any given
time.

Please note that this doesn&rsquo;t affect the file format, i.e., container
filetype, which is always AVI. Rather, it affects the encoding of the video
stream.

Your codec choice is remembered across runs of the program.

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
[Uncompressed](#uncompressed) instead, which leaves out the unused alpha
channel.

#### Uncompressed

If you want uncompressed video (where each frame is a bitmap), you should use
this. It uses SharpAvi&rsquo;s `UncompressedVideoEncoder`. As the [SharpAvi
wiki](https://github.com/baSSiLL/SharpAvi/wiki/Using-Video-Encoders#creating-video-encoder)
says:

> The simplest [encoder] is
[UncompressedVideoEncoder](https://github.com/baSSiLL/SharpAvi/blob/master/SharpAvi/Codecs/UncompressedVideoEncoder.cs).
It does no real encoding, just flips image vertically and converts BGR32 data
to BGR24 data to reduce the size.

Video files created this way are quite large, if you&rsquo;re recording for
more a few seconds. If you&rsquo;re keeping videos you create this way, you may
want to encode them with a compressed codec afterwards. If your hard disk is
slow, you may experience lag while encoding this way. It&rsquo;s still better
than [Raw (frame copy)](#raw-frame-copy), though.

#### Motion JPEG

[Motion JPEG](https://en.wikipedia.org/wiki/Motion_JPEG) is VidDraw&rsquo;s
default encoding. Each frame is converted to and stored as a JPEG image. This
uses SharpAvi&rsquo;s `MotionJpegVideoEncoderWpf`. From the [SharpAvi
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

If the [x264vfw](https://sourceforge.net/projects/x264vfw/) codec (of the same
architecture as the VidDraw process) [is
installed](#optional-get-x264vfw-for-h-264-support), the menu item for
[H.264](https://en.wikipedia.org/wiki/Advanced_Video_Coding) is unfaded. It
isn&rsquo;t the default because VidDraw doesn&rsquo;t ship that codec and you
may not already have it, and because there are [configuration choices for you
to make](#a-problem-with-H-264-in-viddraw) and [playback issues to wrangle
with](#video-files-are-not-always-playable-on-all-players).

Still, I recommend encoding in H.264 if you can, because it produces higher
quality than [Motion JPEG](#motion-jpeg) at a small fraction of the file size
(*even* if you use
[`--keyint 1`](#configure-x264vfw-to-make-every-frame-a-keyframe)).

The [SharpAvi
wiki](https://github.com/baSSiLL/SharpAvi/wiki/Using-Video-Encoders#creating-video-encoder)
says:

> Finally,
[Mpeg4VideoEncoderVcm](https://github.com/baSSiLL/SharpAvi/blob/master/SharpAvi/Codecs/Mpeg4VideoEncoderVcm.cs)
does MPEG-4 encoding using *Video for Windows* (aka *VfW*) or *Video
Compression Manager* (aka *VCM*) compatible codec installed on the system.
>
> Currently tested codecs include **Microsoft MPEG-4 V2** and **V3**,
[Xvid](https://www.xvid.com/download/),
[DivX](http://www.divx.com/en/software/divx) and
[x264vfw](http://sourceforge.net/projects/x264vfw/files/). Unfortunately, some
of them have only 32-bit versions, others produce errors in 64 bits. The only
codec which looks to work reliably in 64 bits is **x264vfw64**. For **x264vfw**
(both 32- and 64-bit), it is recommended to check option **Zero Latency** in
its settings to prevent picture freezes.

### Other Stuff in the Menu

#### Clear Canvas

If the canvas has not been drawn on since opening SharpAvi, or since the last
time it was cleared, then the &ldquo;Clear Canvas&rdquo; option is unfaded.
This clears the canvas, making all pixels white, as they started.

#### Pick Color&hellip;

The pen color is black by default. &ldquo;Pick Color&hellip;&rdquo; opens a
color picker in which you can choose another color. The color picker is based
on
[`ColorDialog`](https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.colordialog?view=net-5.0),
which wraps the [standard Windows color dialog
box](https://docs.microsoft.com/en-us/windows/win32/dlgbox/color-dialog-box)
and, as such, allows you to define custom color presets. VidDraw saves your
custom colors, so they are available if you quit VidDraw and run it again. Your
custom VidDraw colors do not appear in any other programs&rsquo; color dialogs.

(Unfortunately, defining custom colors but cancelling out of the color picker
discards the new custom colors&mdash;you have to click &ldquo;OK.&rdquo; Since
this is what users familiar with the Windows color dialog expect, I don&rsquo;t
think I should try to change it for VidDraw&rsquo;s color picker.)

#### Open Videos Folder

Clicking &ldquo;Open Videos Folder&rdquo; opens the destination folder where
VidDraw saves videos. This is your per-user `Videos` (or `My Videos`)
directory.

This is handy if you meant to click to activate a toast notification after
recording, but dismissed it instead.

In the strange case that you record a video and, while VidDraw is still
running, change the location of your per-user `Videos` folder, this goes to the
new place, not the only one. (The new location is the destination for any
further videos VidDraw saves, even during the same run of the program.)

#### Download x264vfw<br>*or* Configure x264vfw (x64)<br>*or* Configure x264vfw (x86)

If x264vfw is not installed, or it is installed but not for the same
architecture as the VidDraw process, then VidDraw cannot use it. In this case,
the menu contains a &ldquo;Download x264vfw&rdquo; item that opens the x264vfw
download page in your web browser.

If x264vfw is installed for the architecture of the VidDraw process, then
VidDraw can use it, and the menu contains either a &ldquo;Configure x264vfw
(x64)&rdquo; or &ldquo;Configure x264vfw (x86)&rdquo; item, depending on the
architecture of the VidDraw process (i.e., whether it&rsquo;s running as a
64-bit process or a 32-bit process, respectively).

Clicking &ldquo;Configure x264vfw&rdquo; opens x264vfw&rsquo;s own
configuration dialog. This dialog is not part of VidDraw, and configuration
changes you make in in affect x264vfw on your whole system; they are not
specific to VidDraw. But they *are* specific to the 64-bit (x64) or 32-bit
(x86) version of x264vfw. By default, all remotely recent versions of x264vfw
install both the 64-bit and 32-bit codecs. They must be configured separately.

#### About VidDraw&hellip;

Clicking &ldquo;About VidDraw&hellip;&rdquo; displays this README in
VidDraw&rsquo;s built-in help browser.

## Known Bugs

### An indirect dependency has confusing licensing.

VidDraw has [win32metadata](https://github.com/microsoft/win32metadata) as an
indirect dependency, via [CsWin32](https://github.com/microsoft/CsWin32).
win32metadata&rsquo;s [GitHub
repository](https://github.com/microsoft/win32metadata) is [MIT
licensed](https://github.com/microsoft/win32metadata/blob/master/LICENSE), but
its [NuGet
package](https://www.nuget.org/packages/Microsoft.Windows.SDK.Win32Metadata)
shows the [Windows SDK license](https://aka.ms/WinSDKLicenseURL). This [is
intentional](https://github.com/microsoft/win32metadata/issues/387).

I&rsquo;m unclear on what, if any, actual restrictions this imposes on how the
source code, or a compiled binary, of VidDraw, can be used. I intend that
VidDraw be both permissively licensed and GPL-compatible. Adding the CsWin32
package prompted for win32metadata license acceptance. Even if this
doesn&rsquo;t conflict with those goals, I fear it may chill reuse and
adaptation of VidDraw, unless clarified. So this should either be clarified, or
the CsWin32 dependency removed (which would actually not be too hard). This
should be done before the release of alpha 2 and preferably before the release
of alpha 1.

### Sometimes there is an initial lag on the first recording.

This seems to happen mainly while debugging&mdash;even a debug build that is
being run (via `dotnet run` or by directly running the compiled executable) is
most often free of it. But it seems to happen occasionally even outside of
debugging.

This is annoying because, when it happens, it usually results in a straight
line segment appearing on the canvas (and in the video) that the user
didn&rsquo;t intend to draw.

VidDraw could do some operations asynchronously that it now does on the UI
thread. Most or all file I/O could, and should, be asynchronous. Implementing
this will require thought about what to do in some race conditions that cannot
currently happen. I don&rsquo;t know if that would be sufficient to fix this
bug.

### Video files are not always playable on all players.

[As detailed above](#a-problem-with-H-264-in-viddraw), when H.264 encoding is
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
drawing](#menu-items-are-cumbersome-to-access-while-drawing) as well.

### The pen thickness should be adjustable in some way.

Being able to vary the pen&rsquo;s size would make drawing more fun.

This could be done by holding down modifier keys (<kbd>Shift</kbd>,
<kbd>Ctrl</kbd>, <kbd>Alt</kbd>) and/or by selecting something in the menu.

### User experience on older Windows could be better.

Although it is not a goal for VidDraw to fully support any versions of Windows
older than 10.0.17763, it might be good to come up with some other behavior
than immediately opening the destination folder each time on systems that
don&rsquo;t support toast notifications.

Even older Windows systems, such as Windows 7, have very thin title bars, at
least when styling is turned off or classic styling is used. This makes the
border around the canvas (which turns red to indicate recording) look
excessively thick. It&rsquo;s probably not worth it to detect and handle this
situation, but perhaps there&rsquo;s some simple way.

## Dependencies

VidDraw uses the following libraries and fonts. Thanks go to the authors and
contributors to all these projects&mdash;and especially to Vasili Maslov for
writing SharpAvi. Some of these dependencies included in this repository while
others are retrieved by NuGet.

This list is in alphabetical order. Entries for libraries included in this
repository contain *&ldquo;[included]&rdquo;* links to their subdirectories.
Links to each dependency&rsquo;s detailed licensing information are given on
the second line.

- [clipboard.js](https://clipboardjs.com/) 2.0.8 by Zeno Rocha \[included\]\
  [MIT License](https://github.com/zenorocha/clipboard.js/blob/master/LICENSE)
  (local, inline)
- [CsWin32](https://github.com/microsoft/CsWin32) 0.1.422-beta by Andrew Arnott
  / Microsoft
  \[[nuget](https://www.nuget.org/packages/Microsoft.Windows.CsWin32)\]\
  [MIT License](https://github.com/microsoft/CsWin32/blob/main/LICENSE)
  (inline)
- [*Fork me on GitHub* CSS
  ribbon](https://simonwhitaker.github.io/github-fork-ribbon-css/) 0.2.3 by
  Simon Whitaker \[included\]\
  [MIT
  License](https://github.com/simonwhitaker/github-fork-ribbon-css/blob/0.2.3/LICENSE)
  (local, inline)
- [Json.NET](https://www.newtonsoft.com/json) (Newtonsoft.Json) 13.0.1 by James
  Newton-King \[[nuget](https://www.nuget.org/packages/Newtonsoft.Json)\]\
  [MIT
  License](https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md)
  (inline)
- [kbd](https://auth0.github.io/kbd/) by Auth0 \[included\]\
  [MIT License](https://github.com/auth0/kbd/blob/gh-pages/LICENSE) (local,
  inline)
- [Milligram](https://milligram.io/) 1.4.1 by CJ Patoilo \[included\]\
  [MIT License](https://github.com/milligram/milligram/blob/master/license)
  (local, inline)
- [normalize.css](https://necolas.github.io/normalize.css/) 8.0.1 by Nicolas
  Gallagher and Jonathan Neal \[included\]\
  [MIT License](https://github.com/necolas/normalize.css/blob/8.0.1/LICENSE.md)
  (local, inline)
- [Open Sans](https://fonts.google.com/specimen/Open+Sans) and [Open Sans
  Condensed](https://fonts.google.com/specimen/Open+Sans+Condensed) (fonts) by
  Steve Matteson \[included\]\
  [Apache License, Version
  2.0](https://www.apache.org/licenses/LICENSE-2.0.html) (local, local, inline)
- [SharpAvi.Net5](https://github.com/EliahKagan/SharpAvi) 2.1.2-rc, my fork of
  [SharpAvi](https://github.com/baSSiLL/SharpAvi) 2.1.2 by Vasili Maslov
  \[[nuget](https://www.nuget.org/packages/SharpAvi.Net5)\]\
  [MIT License](https://github.com/EliahKagan/SharpAvi/blob/net5/LICENSE.md)
  (inline)
- [Windows Community
  Toolkit](https://docs.microsoft.com/en-us/windows/communitytoolkit/) &ndash;
  [Notifications](https://github.com/windows-toolkit/WindowsCommunityToolkit/tree/main/Microsoft.Toolkit.Uwp.Notifications)
  7.0.1 by .NET Foundation and Contributors
  \[[nuget](https://www.nuget.org/packages/Microsoft.Toolkit.Uwp.Notifications)\]\
  [MIT
  License](https://github.com/windows-toolkit/WindowsCommunityToolkit/blob/main/license.md)
  (inline)

VidDraw is a C# program targeting [.NET](https://dotnet.microsoft.com/) 5 on
Windows. It uses [Windows Forms](https://github.com/dotnet/winforms), and it
makes calls to the [Windows API](https://en.wikipedia.org/wiki/Windows_API) via
libraries such as SharpAvi and Windows Forms as well as directly. These and
other components that I believe are considered part of the framework (.NET) or
operating system (Windows) are not listed above.

Indirect dependencies (dependencies of the above-listed dependencies that are
either included in them or otherwise resolved through them) are also not
listed.

### VidDraw and x264vfw

VidDraw does not depend on [x264vfw](https://sourceforge.net/projects/x264vfw/)
but is capable of using it [if it is
installed](#optional-get-x264vfw-for-h-264-support). x264vfw is a
[VfW](https://en.wikipedia.org/wiki/Video_for_Windows) codec for
[H.264](https://en.wikipedia.org/wiki/Video_coding_format) by Anton Mitrofanov
and other authors, derived from
[x264](https://www.videolan.org/developers/x264.html). Since, when installed
and selected in VidDraw, it greatly reduces file size while preserving quality,
I&rsquo;m thankful to all the developers of x264vfw and x264 for the work
they&rsquo;ve done. Per its
[COPYING](https://sourceforge.net/p/x264vfw/code/HEAD/tree/trunk/COPYING) file
and license headers in most of [its source code
files](https://sourceforge.net/p/x264vfw/code/HEAD/tree/trunk/), x264vfw is
licensed under [the GNU GPL v2 or
later](https://spdx.org/licenses/GPL-2.0-or-later.html).

VidDraw uses x264vfw via SharpAvi in a manner analogous to how any VfW codec
may be used, doesn&rsquo;t itself include the codec, and interacts with it [at
arm&rsquo;s
length](https://www.gnu.org/licenses/gpl-faq.en.html#MereAggregation) (VfW
codecs are shared libraries, but as I understand it, programs that use them
don&rsquo;t link to them or directly call API functions that they define). So I
believe the GPL does not require that VidDraw or any of the libraries VidDraw
uses be offered under the GPL or have a GPL-compatible license.

Please note that, for practical and ideological reasons unrelated to x264vfw, I
still intend VidDraw to be GPL-compatible! If it is not, or even if it [appears
not to be](#an-indirect-dependency-has-confusing-licensing), I&rsquo;d consider
that a serious bug.
