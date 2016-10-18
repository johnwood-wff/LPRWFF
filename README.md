# LPRWFF
Optasia LPR Interface to WFF

Listens on a UDP port, and any messages received it will send to the VMS as a plugin function.

Note the plugin function has to be named with an underscore prefix so that it can be invoked from the Internet, this is a WFF security restriction.

The settings are controlled by a file called settings.cfg (ini style), in the same folder as the executable, that can contain:


Server: The URL of the VMS server to send the message to.

User: The user to login to the VMS as

Password: The password to use when logging into the VMS.

LPRPort: The UDP port to listen on, defaults to 8002.

