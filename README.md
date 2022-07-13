# Cubemap Maker
Create 360 Cubemaps from within Unity games for use in The Cyber Grind

![4-2 Cubemap](img/using_cerberus.png)

## Usage

Press "F10" to capture a Cubemap

Press "F11" to capture a Cubemap with a transparent skybox

Holding down "Left Shift" while capturing a cubemap on The Cyber Grind will capture a [Second Player Cubemap](#second-player-cubemaps-the-cyber-grind)

The image will be saved to The ULTRAKILL's Cyber Grind Skybox folder by default, but in edge cases where the plugin could not find this folder (or you aren't planning on using this for ULTRAKILL), it will be saved in a folder in your Pictures directory named "Cubemaps"

You can find the `config.cfg` in `%AppData%/CubemapMaker/`. There you can change the output path, the capture key, the orientation of the cubemap, the output resolution, and whether or not to include the UI in a [Second Player Cubemap](#second-player-cubemaps-the-cyber-grind).

## Second Player Cubemaps (The Cyber Grind)

While in The Cyber grind, if you hold down shift while capturing a cubemap, a special "Second Player" cubemap will be created.

These Second Player cubemaps are supposed to give the illusion that a second player is playing beside you. You could think of them as a rival, or as moral support. The choice is yours, my friend!

[![Second Player Cubemap](img/competitor.png)](https://momento360.com/e/u/b6f03b9e647f49dcb43052df860e0e4e?utm_campaign=embed&utm_source=other&heading=-156.18&pitch=21.31&field-of-view=77&size=medium)

## Orientation Settings

There are 3 orientation settings available:

**- Yaw (Default):** This will capture the cubemap in the direction you're facing but will keep the world upright

**- Accurate:** This will accurately capture rotation in the cubemap, so wherever you are looking will be the forward position. This can create some very disorienting cubemaps, so use with caution! (Or don't, I don't really care tbh)

**- None:** This won't take the camera's orientation into account at all

## Examples (Click to view in 360)

The following was taken in ULTRAKILL's 4-2:

[![4-2 Cubemap](img/greed_map.png)](https://momento360.com/e/u/ae9b53244fe947fea807478156c30b5d?utm_campaign=embed&utm_source=other&heading=0&pitch=0&field-of-view=75&size=medium)

Here is another of the Cerberus Boss Room:

[![Cerberus Cubemap](img/cerberus_map.png)](https://momento360.com/e/u/d5900bba103a4a329c96867bd9a33942?utm_campaign=embed&utm_source=other&heading=0&pitch=0&field-of-view=75&size=medium)

While this plugin was developed with ULTRAKILL in mind, this mod works with most Unity games. This image was taken in the main menu of Muck:

[![Muck Main Menu Cubemap](img/muck_map.png)](https://momento360.com/e/u/b4502e9ee399400a94f2523a57ad6293?utm_campaign=embed&utm_source=other&heading=0&pitch=0&field-of-view=75&size=medium)