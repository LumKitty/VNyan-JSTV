## VNyan -> Joystic.tv integration

Early alpha software... good luck
(Also I'm on holiday from 21st May - 2 June. You're welcome to send me bug reports, but please understand that I may not respond quickly)

### Setup - JSTV website:
1. Visit https://joystick.tv/applications
2. Create a new bot
3. Fill in a username. This will be displayed in chat and should be specific to you, not something generic like "VNyan"
4. Make a note of the Application ID, Client ID and Client Secret
   <img width="870" height="1065" alt="image" src="https://github.com/user-attachments/assets/0769e318-3d1c-4b94-8d54-9804ae5ae308" />
5. Give it all the permissions for now, I'll figure out what it actually needs later!
6. Set the OAuth Redirect URL to http://localhost:6969

### Setup - VNyan
6. Copy VNyan_JSTV.dll into .\VNyan\Items\Assemblies
7. Start VNyan, then close it
8. Open JSTV.json fill in the Application ID, Client ID and Client Secret you noted at step 4

### Triggers you can recieve
!commands will generate a VNyan trigger named `_jscmd_commands`  
text1 - Username  
text2 - Arguments  
num1 - If the arguments are a number, it will be here, otherwise zero  

Example:  
!nut 50 -> `_jscmd_nut` (Text2: 50 Num1: 50)  

`_lum_jstv_chat` generated on every chat message  
text1 - Username  
text2 - Message  

`_jsevent_<eventtype>` generated in response to various JSTV events. Listed at: https://support.joystick.tv/developer_support/#connecting-the-bot  
text1 - Username
text2 - Item name (e.g. tip menu or wheel prize)
num1 - Value as appropriate (e.g. tip amount, or number of followers)

### Triggers you can send:  
`_lum_jstv_sendchat` - Send a chat message  
text1 - Message to send. This will be sent under the username you set in step 3
