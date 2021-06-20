# SimpleUnityTCP 🖧 
*Created by Eric Batlle Clavero*


A simple **demo-project** to show how **TCP** communication works on **Unity** environment, builded with **C#** and using [System.Net.Sockets](https://docs.microsoft.com/es-es/dotnet/api/system.net.sockets?view=netframework-4.7.2). 

The main porpouse of this repo is to show the TCP communication on runtime, but also to provide a pretty commented and clear code so everyone else that wants to implement that kind of communication will be able without wasting a lot of time.

## Video-Example 📲

<p>
  <img src="example_app.gif" alt="example_app gif"/>
</p>

## Donations are appreciated! 💸
*Remember that are many ways to say thank you.*

If this repository has been helpful remember to star it and consider buying me a coffee! 😀 
<p>
<a href="https://www.buymeacoffee.com/ebatlleclavero" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/default-blue.png" alt="Buy Me A Coffee" width="144.6" height="34"></a>
</p>

If you like my general work and contributions consider [sponsoring me on Github](https://github.com/sponsors/EricBatlle). 

But if you just want to donate straightforward, I also have [PayPal.me](https://paypal.me/EricBatlleClavero?locale.x=es_ES).

## How to Use 💻
If you only want to see the app working, just run the ``SimpleTCP.exe`` which is located on the ``build`` directory.

If you want to open the project, you will need to have **Unity** installed with the **version 2017 or higher**.

If you only want to scratch the code, either inside the unity project or simply dragging the **.cs** classes on your editor, you have to watch on to this classes, which are located on ``Assets\Scripts``:

* ``Server.cs``
* ``Client.cs``

## More Explanations 📡
**Note:** Unity do not allow the *save-use* of **Multi-Threading**, so the application is using **Co-routines** instead.

The stablished communication and workflow stablished right now follows the next order:

* **Start the Server** and let it waiting for client connections.
* **Start the Client** and connect it to the server.
* **Server/Client waits** for client/server messages...
* **Client/Server sends** any type of message to the server/client.
* **Server/Client sends** recives the message from the client/server.
* **Server/Client waits** for new client/server messages...

* **In Case Client sends "Close" to Server**
* **Client sends "Close" to Server**, and waits a timeOut to disconnect from it.
* **Server recives** that *Close* message and **closes the connection** with the client.
* **Server** waits for new incoming client connections.

* **Server shuts down** (Optional)

Keep in mind that this "Close" system is not the typical four-way handshake used in TCP, but it's builded like this to keep it simple and understandable.
You can learn more about TCP Connection termination [here](https://en.wikipedia.org/wiki/Transmission_Control_Protocol#:~:text=The%20connection%20termination%20phase%20uses,end%20acknowledges%20with%20an%20ACK.).