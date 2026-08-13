## VNyan -> Joystick.tv integration

This is a beta. It works for me and several others, but no guarantees.
With the announcement that VNyan will be getting official Joystick support in 1.7.2 this plugin won't see too much new development. I'll help with any issues and fix any bugs that you find, but won't be updating it to API 2.0 or anything like that.
When VNyan 1.7.2 releases, I recommend using its native support instead of this plugin

This plugin is completely free and open source. I will never ask you for money, but if it's useful consider dropping me a follow or a raid some time :3

### Setup - JSTV website:
1. Visit https://joystick.tv/applications
2. Create a new bot
3. Fill in a username. This will be displayed in chat and should be specific to you, not something generic like "VNyan"
4. Make a note of the Application ID, Client ID and Client Secret
   <img width="870" height="1065" alt="image" src="https://github.com/user-attachments/assets/0769e318-3d1c-4b94-8d54-9804ae5ae308" />
5. Give it all the permissions for now, I'll figure out what it actually needs later!
6. Set the OAuth Redirect URL to http://localhost:6969

### Setup - VNyan
6. Copy VNyan_JSTV.dll and websocket-sharp-core.dll into .\VNyan\Items\Assemblies
7. Make sure you unblocked them (right click -> properties -> unblock)
8. Make sure you enabled plugins in VNyan's misc settings
9. Start VNyan, then close it
10. Open JSTV.json in your VNyan profile directory
11. Fill in the Application ID, Client ID and Client Secret you noted at step 4

### Triggers you can recieve
!commands will generate a VNyan trigger named `_jscmd_commands`  
text1 - Username  
text2 - Arguments  
num1 - If the arguments are a number, it will be here, otherwise zero  

Example:  
!nut 50 -> `_jscmd_nut` (Text2: 50 Num1: 50)  

`_jschat` generated on every chat message  
text1 - Username  
text2 - Message  

`_jsevent_<eventtype>` generated in response to various JSTV events. Listed at: https://support.joystick.tv/developer_support/#connecting-the-bot  
text1 - Username  
text2 - Item name (e.g. tip menu or wheel prize)  
num1 - Value as appropriate (e.g. tip amount, or number of followers)  
Some events will also send num2 and num3. These will be documented eventually. For now you will need to read JSMessage.cs to understand what is being sent and where!  
Status of event support is in Events.xlsx / Events.png. Green = This should work. Yellow = This might work. Red = Not implemented

### Triggers you can send:  
`_lum_jstv_sendchat` - Send a chat message  
text1 - Message to send. This will be sent under the username you set in step 3

`_lum_jstv_sendwhisper` - Send a chat message  
text1 - Message to send. This will be sent under the username you set in step 3  
text2 - Username to send to

`_lum_jstv_connect` - Connect to joystick.tv  

`_lum_jstv_disconnect` - Disconnect from joystick.tv

### Shameless self promo
https://twitch.tv/LumKitty  
https://joystick.tv/u/LumKitty

### Current implementation state for events
Please let me know if these events (including all their values) are working for you
<img src="https://github.com/LumKitty/VNyan-JSTV/blob/master/Events.png?raw=true">
