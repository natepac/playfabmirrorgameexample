# PlayFab / Mirror / Unity - Setup and Example Game

#### April 8th 2020

#### Nate Pacyga - nate@trycatchgames.com

#### Special Thanks: Brandon Phillips of Uproot Studios https://uprootstudios.com/

## For a Google Doc version of this with screenshots, go to: https://bit.ly/2xpIcsV

### Why use PlayFab and Mirror?

I had personal experience with how useful PlayFab can be with my indie title Turbo Town a few years ago. Though I didn’t use the multiplayer API and hosting features, I used about everything else they had to offer and my experience was exemplary. When I wanted to dig back into multiplayer gaming and researching the options I wanted to see what Mirror was all about as I researched it briefly a couple years ago. The community around Mirror is a passionate and helpful bunch and as I unfortunately stumbled through getting my multiplayer server totally working on PlayFab and I wanted to write this document. Coupling your multiplayer hosting with PlayFab is a no-brainer decision though. The features and liveops features PlayFab provides devs out of the box is nothing short of impressive and will save you thousands of hours of work. Add in using Mirror as your Unity multiplayer framework and you have a great starting point to build the multiplayer game of your dreams. Oh that and it’s free to get started and get a build rockin’ and rollin’ servin’ up multiplayer games.

### What this document is:

This document is meant to be a simple setup and go document for getting started with Mirror and PlayFab server hosting. Hopefully it will answer your questions on how to get going and started on your dream multiplayer game!

### What this document isn’t:

This is not a one size fits all document. It will get you started, but from here I expect you to explore, make mistakes and try things out. 

### What this document assumes:

That you are a competent and experienced game developer. This is not a beginner document. I won’t be covering obvious project set up and details. This is meant for a TLDR impatient developer like myself that just needs answers quickly.

### What you will need:

* Download PlayFab SDK and get familiar with it
* Download Mirror from Github or Unity Asset Store
* Download GSDK or use the starter project
* Clone my example game repo

### PlayFab Web Console Setup:

* You will need to enable your client to start games. When you ship your game you’ll disable this, but for now this is just an easy way to see something working.
* Login to your PlayFab account
* Click on the Settings gear
* In Settings, click on the API Features tab and enable “Allow Client to Start Games”

### Building a PlayFab hosted Server:

Once you have cloned the project, open it up and feel free to get familiar with the code in the Scripts folder and open up the Game scene. The Hierarchy is meant to be simple and straightforward with most of the used scripts owning their own GameObject. 

The entire build control is done from the CONFIGURATION GameObject in the Hierarchy. This is intentional. Select “REMOTE_SERVER” from the drop down Build Type menu in the Inspector when the object is selected. Nothing else needs to be set to build out a server to be hosted on PlayFab. 

### Once this is set go to the Build Settings menu:

We are going to do a Windows build and it should look like this. 

(See Google Doc for screenshot)

Pump out a Server build, then select all the FILES (not the folder) and zip them up. We will be uploading the zip to PlayFab. 

### Uploading a Server build to PlayFab:

Go to your PlayFab web console and click on the Multiplayer tab on the left. With the new pricing, you will have to enter in a credit card number to enable multiplayer. It should remain free while you are developing, but I suppose it depends on your use case and I don’t work for PlayFab so I can’t speak to that. 

Once you are able to add a build, click on the New Build button. You will want to set up your configuration like these screenshots: 

(See Google Doc link above)

Read the official PlayFab documentation on this subject.

### A few notes:

There is no progress bar for uploading your Zip. Please allow enough time for it to complete uploading or you will get a cryptic error at the top of the page. 

Speaking of errors, lots of the errors don’t make sense on this page. If you get one, just make sure your details you entered are correct. 

Once you hit saved, you may see “Initialized”. If that sticks around for more than 30 minutes, I would go into your server settings and add a region, like West. Then it should make it deploy. No idea why this works currently.

I would explain more here, but just reading the documentation link above should answer your questions.

If everything went well and uploaded. You should see “Initialized” or “Deploying”. Wait for a bit till you see “Deployed” then you should see the status of “Healthy” on that entry. If you don’t, you may have to redeploy and make sure your details are correct.

## Wait till you see a status of “Deployed” before moving on!

### Connecting with a Remote Client:
Really this is just a regular client. I just call it “remote” because you can also build a client that connects to a local server. 

Select the Configuration GameObject in the Hierarchy and make sure the IP and Port are blank (they should be by default). However, we do need the BuildId of your server that you uploaded and deployed to PlayFab. Select the server build in the PlayFab Multiplayer Web Console. It looks like this:

(See Google Doc link above)

Once you have BuildId, copy and paste it into the BuildId of the Configuration GameObject. Then hit Play in the Unity Editor. Once launched into Game mode, make sure the Console is open then click the “Login” button in the upper left corner. You should see the Console output a number of lines and details. Find the one that says “**** ADD THIS TO YOUR CONFIGURATION ****” and copy the IP address and Port details. Oh and you should also see a Player GameObject spawn in the scene as you should be connecting to the remote server you just deployed (this can take up to 20 or so seconds sometimes, this is likely because of cold starts on your first login). Your screen should look like: 

(See Google Doc link above)

With your copied console line, enter in the IP address and Port into the Configuration fields. 

Your Configuration object should look something like this:

(See Google Doc link above)

You should be able to make a build of the Remote Client. Go to Build Settings… and make sure that “Server Build” is unchecked and Build. 

Once built, you should be able to open up the exe you put out and connect with it to your PlayFab hosted server after you click the Login button!

The “Players” are just spawned white cubes in a 5x3 grid. You may have a player spawn on top of another. This is just a quick and dirty player spawn example to show that others are connecting. If you happen to hit Login and don’t see anything make sure the window is sized enough and you can also move the camera back by holding the “S” key, as the camera has a Simple Camera Controller script on it. 

*****************************************************

WARNING: YOU WILL PAY FOR STANDBY SERVERS!!! Now that your CC number is registered at PlayFab you will be responsible for any standby server costs. Please refer to the server costs see here:

https://docs.microsoft.com/en-us/gaming/playfab/features/multiplayer/servers/billing-for-thunderhead

When you are not testing, set your Standby Servers to 0. 

*****************************************************

### A few notes and tips:

Make sure your server has a timeout script on it with Application.Quit() my ServerStartUp script has an example of this. 
It may seem silly that I call RequestMultiplayerServer to get the IP and Port. This is what is used to get this information as it’s not on the web console. You can get a IP and Port for the VM, but this isn’t what’s used on a deployed game server. 
If you keep using RequestMultiplayerServer you will allocate those servers and will have to wait for them to shutdown. That is why we copy the IP and Port of one server and keep connecting to it. 

### NOTE: 
I am by no means an expert with all this yet. It took me a few weeks of off and on poking around with this to make it work in my free time. I am still digging into this and if anyone has any feedback please feel free to leave comments on this document or get in touch with me: nate@trycatchgames.com
 
### Additional Notes:

You should be using the Linux hosting option when you launch. Brandon wrote a great tool to help assist you in updating your Linux build. Please note, if you’re on Windows, you’ll need Windows 10 Pro for Docker. 
https://github.com/bphillips09/PFAdmin

When you’re ready to allocate servers properly, you’ll use Matchmaking:
https://docs.microsoft.com/en-us/rest/api/playfab/multiplayer/matchmaking

Vis2k’s tutorial about server hosting:
https://noobtuts.com/unity/unet-server-hosting

Server auto-scaling:
https://docs.microsoft.com/en-us/gaming/playfab/features/multiplayer/servers/dynamic-standby
