# Sock Chat Protocol Information
The protocol operates on a websocket in text mode. Messages sent between the client and server are a series of concatenated strings delimited by the vertical tab character, represented in most languages by the escape sequence `\t` and defined in ASCII as `0x09`.
The first string in this concatenation must be the packet identifier, sent as an `int`.

Newer versions of the protocol are implemented as extensions, a client for Version 1 should have no trouble using a server built for Version 2 as long as authentication is understood.

The current stable version of the protocol is **Version 1**.


## Types

### `bool`
A value that indicates a true or a false state. `0` represents false and anything non-`0` represents true, please stick to `1` for representing true though.


### `int`
Any number ranging from `-9007199254740991` to `9007199254740991`, `Number.MAX_SAFE_INTEGER` and `Number.MIN_SAFE_INTEGER` in JavaScript.


### `string`
Any printable unicode character, except `\t` which is used to separate packets.


### `timestamp`
Extends `int`, contains a second based UNIX timestamp.


### `channel name`
A `string` containing only alphanumeric characters (any case), `-` or `_`.


### `session id`
A `string` containing only alphanumeric characters (any case), `-` or `_`.


### `color`
Any valid value for the CSS `color` property.
Further documentation can be found [on MDN](https://developer.mozilla.org/en-US/docs/Web/CSS/color_value).


### `message flags`
Message flags alter how a message should be displayed to the client, these are all `bool` values.
The parts are as follows:

 - Username should appear using a **bold** font.
 - Username should appear using a *cursive* font.
 - Username should appear __underlined__.
 - A colon `:` should be displayed between the username and the message.
 - The message was sent privately, directly to the current user.

As an example, the most common message flagset is `10010`. This indicates a bold username with a colon separator.


### `user permissions`
User permissions are a set of flags separated by either the form feed character (`\f` / `0x0C`) or a space (<code> </code> / `0x20`).
The reason there are two options is due to a past mixup that we now have to live with.
Which of the methods is used remains consistent per server however, so the result of a test can be cached.

<table>
    <tr>
        <td><code>int</code></td>
        <td>Rank of the user. Used to determine what channels a user can access or what other users the user can moderate.</td>
        <td></td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Indicates whether the user the ability kick/ban/unban others.</td>
        <td></td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Indicates whether the user can access the logs. This should always be <code>0</code>, unless the client has a dedicated log view that can be accessed without connecting to the chat server.</td>
        <td></td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Indicates whether the user can set an alternate display name.</td>
        <td></td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Indicates whether the user can create channel. If <code>0</code> the user cannot create channels, if <code>1</code> the user can create channels but they are to disappear when all users have left it and if <code>2</code> the user can create channels that permanently stay in the channel assortment.</td>
        <td></td>
    </tr>
</table>


## Client Packets
These are the packets sent from the client to the server.

### Packet `0`: Ping
Used to prevent the client from closing the session due to inactivity.

<table>
    <tr>
        <td><code>int</code></td>
        <td>User ID</td>
        <td></td>
    </tr>
    <tr>
        <td><code>timestamp</code></td>
        <td>Time the packet was sent to the server</td>
        <td>Added in version 2</td>
    </tr>
</table>


### Packet `1`: Authentication
Takes a variable number of parameters that are fed into the authentication script associated with the chat.

<table>
    <tr>
        <td><code>...string</code></td>
        <td>Any amount of data required to complete authentication</td>
        <td></td>
    </tr>
</table>


### Packet `2`: Message
Informs the server that the user has sent a message.

Commands are described lower in the document.

<table>
    <tr>
        <td><code>int</code></td>
        <td>User ID</td>
        <td></td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Message text, may not contain <code>\t</code></td>
        <td></td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Name of the target channel</td>
        <td>Added in version 2 through the <code>MCHAN</code> capability. May be omitted to target the last addressed channel.</td>
    </tr>
</table>


### Packet `3`: Capabilities
Informs the server what capabilities this client supports. Capabilities can be adjusted at any time.

Added in Version 2.

<table>
    <tr>
        <td><code>string</code></td>
        <td>A space separated list of capability strings.</td>
        <td></td>
    </tr>
</table>


### Packet `4`: Typing
Informs the currently focussed channel that this client is writing a message.

Added in Version 2 through the <code>TYPING</code> capability.

<table>
    <tr>
        <td><code>int</code></td>
        <td>User ID</td>
        <td></td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Name of the target channel.</td>
        <td></td>
    </tr>
</table>


## Server Packets
These are the packets sent from the server to the client.

### Packet `0`: Pong
Response to client packet `0`: Ping.

<table>
    <tr>
        <td><code>timestamp</code></td>
        <td>Time the packet was sent to the client</td>
        <td>Originally this field contained the string <code>pong</code>, but this value was completely unused and can be safely replaced.</td>
    </tr>
</table>


### Packet `1`: Join/Auth
While authenticated this packet indicates that a new user has joined the server/channel. Before authentication this packet serves as a response to client packet `1`: Authentication.

#### Successful authentication response
Informs the client that authentication has succeeded.

<table>
    <tr>
        <td><code>string</code></td>
        <td><code>y</code></td>
        <td></td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>User ID</td>
        <td></td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Username</td>
        <td></td>
    </tr>
    <tr>
        <td><code>color</code></td>
        <td>Username color</td>
        <td></td>
    </tr>
    <tr>
        <td><code>user permissions</code></td>
        <td>User permissions</td>
        <td></td>
    </tr>
    <tr>
        <td><code>channel name</code></td>
        <td>Default channel the user will join following this packet</td>
        <td></td>
    </tr>
    <tr>
        <td><code>session id</code></td>
        <td>ID of the currently active session</td>
        <td>Added in Version 2</td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Maximum permitted size of a chat message</td>
        <td>Added in Version 2</td>
    </tr>
</table>


#### Failed authentication response
Informs the client that authentication has failed.

<table>
    <tr>
        <td><code>string</code></td>
        <td><code>n</code></td>
        <td></td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>
            Reason for failure.
            <ul>
                <li><code>authfail</code>: Authentication data is invalid.</li>
                <li><code>userfail</code>: Username in use.</li>
                <li><code>sockfail</code>: A connection has already been started (used to cap maximum connections to 5 in SharpChat).</li>
                <li><code>joinfail</code>: User attempting to authenticate is banned.</li>
            </ul>
        </td>
        <td></td>
    </tr>
    <tr>
        <td><code>timestamp</code></td>
        <td>If <code>joinfail</code> this contains expiration time of the ban, otherwise it is empty.</td>
        <td></td>
    </tr>
</table>


#### User joining
Informs the client that a user has joined.

<table>
    <tr>
        <td><code>timestamp</code></td>
        <td>Time the user joined</td>
        <td></td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>User ID</td>
        <td></td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Username</td>
        <td></td>
    </tr>
    <tr>
        <td><code>colour</code></td>
        <td>Username color</td>
        <td></td>
    </tr>
    <tr>
        <td><code>user permissions</code></td>
        <td>User permissions</td>
        <td></td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Sequence ID</td>
        <td></td>
    </tr>
</table>


### Packet `2`: Chat message
Informs the client that a chat message has been received.

<table>
    <tr>
        <td><code>timestamp</code></td>
        <td>Time when the message was received by the server</td>
        <td></td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>
            User ID.
            If <code>-1</code> this message is an informational message from the server and the next field takes on a special form.
        </td>
        <td></td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>
            <p>Message, sanitised by the server</p>
            <p>
                If this is an informational message this field is formatted as follows and concatenated by the form feed character <code>\f</code>, respresented in ASCII by <code>0x0C</code>. Bot message formats are described lower in the document.
                <table>
                    <tr>
                        <td><code>int</code></td>
                        <td>
                            Message type.
                            <ul>
                                <li><code>0</code> for a normal informational message.</li>
                                <li><code>1</code> for an error.</li>
                            </ul>
                        </td>
                    </tr>
                    <tr>
                        <td><code>string</code></td>
                        <td>An id of a string in the legacy language files.</td>
                    </tr>
                    <tr>
                        <td><code>...string</code></td>
                        <td>Any number of parameters used to format the language string.</td>
                    </tr>
                </table>
            </p>
        </td>
        <td></td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Sequence ID</td>
        <td></td>
    </tr>
    <tr>
        <td><code>message flags</code></td>
        <td>Message flags</td>
        <td></td>
    </tr>
    <tr>
        <td><code>channel name</code></td>
        <td>Channel this message was sent in</td>
        <td>Added in Version 2</td>
    </tr>
</table>


### Packet `3`: User disconnect
Informs the client that a user has disconnected.

<table>
    <tr>
        <td><code>int</code></td>
        <td>User ID</td>
        <td></td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Username</td>
        <td></td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>
            One of four disconnect reasons.
            <ul>
                <li><code>leave</code>: The user gracefully left, e.g. "x logged out".</li>
                <li><code>timeout</code>: The user lost connection unexpectedly, e.g. "x timed out".</li>
                <li><code>kick</code>: The user has been kicked, e.g. "x has been kicked".</li>
                <li><code>flood</code>: The user has been kicked for exceeding the flood protection limit, e.g. "x has been kicked for spam".</li>
            </ul>
        </td>
        <td></td>
    </tr>
    <tr>
        <td><code>timestamp</code></td>
        <td>Time when the user disconnected</td>
        <td></td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Sequence ID</td>
        <td></td>
    </tr>
</table>


### Packet `4`: Channel event
This packet informs the user about channel related updates. The only consistent parameter across sub-packets is the first one described as follows.

<table>
    <tr>
        <td><code>int</code></td>
        <td>
            Channel information update event ID.
            <ul>
                <li><code>0</code>: A channel has been created.</li>
                <li><code>1</code>: A channel has been updated.</li>
                <li><code>2</code>: A channel has been deleted.</li>
            </ul>
        </td>
        <td></td>
    </tr>
</table>


#### Sub-packet `0`: Channel creation
Informs the client that a channel has been created.

<table>
    <tr>
        <td><code>channel name</code></td>
        <td>The name of the new channel</td>
        <td></td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Indicates whether the channel is password protected</td>
        <td></td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Indicates whether the channel is temporary, meaning the channel will be deleted after the last person departs</td>
        <td></td>
    </tr>
</table>


#### Sub-packet `1`: Channel update
Informs the client that details of a channel has changed.

<table>
    <tr>
        <td><code>channel name</code></td>
        <td>Old/current name of the channel</td>
        <td></td>
    </tr>
    <tr>
        <td><code>channel name</code></td>
        <td>New name of the channel</td>
        <td></td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Indicates whether the channel is password protected</td>
        <td></td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Indicates whether the channel is temporary, meaning the channel will be deleted after the last person departs</td>
        <td></td>
    </tr>
</table>


#### Sub-packet `2`: Channel deletion
Informs the client that a channel has been deleted

<table>
    <tr>
        <td><code>channel name</code></td>
        <td>Name of the channel that has been deleted</td>
        <td></td>
    </tr>
</table>


### Packet `5`: Channel switching
This packet informs the client about channel switching.

<table>
    <tr>
        <td><code>int</code></td>
        <td>
            Channel information update event ID.
            <ul>
                <li><code>0</code>: A user joined the channel. Sent to all users in said channel.</li>
                <li><code>1</code>: A user left the channel. Sent to all users in said channel.</li>
                <li><code>2</code>: The client is to forcibly join a channel.</li>
            </ul>
        </td>
        <td></td>
    </tr>
</table>


#### Sub-packet `0`: Channel join
Informs the client that a user has joined the channel.

<table>
    <tr>
        <td><code>int</code></td>
        <td>User ID</td>
        <td></td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Username</td>
        <td></td>
    </tr>
    <tr>
        <td><code>color</code></td>
        <td>Username color</td>
        <td></td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Sequence ID</td>
        <td></td>
    </tr>
</table>


#### Sub-packet `1`: Channel departure
Informs the client that a user has left the channel.

<table>
    <tr>
        <td><code>int</code></td>
        <td>User ID</td>
        <td></td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Sequence ID</td>
        <td></td>
    </tr>
</table>


#### Sub-packet `2`: Forced channel switch
Informs the client that it has been forcibly switched to a different channel.

<table>
    <tr>
        <td><code>channel name</code></td>
        <td>The name of the channel that the user has been switched to</td>
        <td></td>
    </tr>
</table>


### Packet `6`: Message deletion
Informs the client that a message has been deleted.

<table>
    <tr>
        <td><code>int</code></td>
        <td>Sequence ID of the deleted message</td>
        <td></td>
    </tr>
</table>


### Packet `7`: Context information
Informs the client about the context of a channel before the client was present.

<table>
    <tr>
        <td><code>int</code></td>
        <td>
            Context event ID.
            <ul>
                <li><code>0</code>: Users present in the current channel.</li>
                <li><code>1</code>: A message already in the channel, occurs more than once per channel.</li>
                <li><code>2</code>: Channels on the server.</li>
            </ul>
        </td>
        <td></td>
    </tr>
</table>

#### Sub-packet `0`: Existing users
Informs the client about users already present in the channel.

<table>
    <tr>
        <td><code>int</code></td>
        <td>Amount of users present in the channel</td>
        <td></td>
    </tr>
    <tr>
        <td><code>Context User</code></td>
        <td>An amount of repetitions of this object based on the number in the previous param, the object is described below</td>
        <td></td>
    </tr>
</table>

##### Context User object

<table>
    <tr>
        <td><code>int</code></td>
        <td>User ID</td>
        <td></td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Username</td>
        <td></td>
    </tr>
    <tr>
        <td><code>color</code></td>
        <td>Username color</td>
        <td></td>
    </tr>
    <tr>
        <td><code>user permissions</code></td>
        <td>User permissions</td>
        <td></td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Whether the user should be visible in the users list</td>
        <td></td>
    </tr>
</table>


#### Sub-packet `1`: Existing message
Informs the client about an existing message in a channel.

<table>
    <tr>
        <td><code>timestamp</code></td>
        <td>Time when the message was received by the server</td>
        <td></td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>User ID</td>
        <td></td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Username</td>
        <td></td>
    </tr>
    <tr>
        <td><code>color</code></td>
        <td>Username color</td>
        <td></td>
    </tr>
    <tr>
        <td><code>user permissions</code></td>
        <td>User permissions</td>
        <td></td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Message text, functions the same as described in Packet <code>2</code>: Chat message</td>
        <td></td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Sequence ID</td>
        <td></td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Whether the client should notify the user about this message</td>
        <td></td>
    </tr>
    <tr>
        <td><code>message flags</code></td>
        <td>Message flags</td>
        <td></td>
    </tr>
</table>


#### Sub-packet `2`: Channels
Informs the client about the channels on the server.

<table>
    <tr>
        <td><code>int</code></td>
        <td>Amount of channels on the channel</td>
        <td></td>
    </tr>
    <tr>
        <td><code>Context Channel</code></td>
        <td>An amount of repetitions of this object based on the number in the previous param, the object is described below</td>
        <td></td>
    </tr>
</table>

##### Context Channel object

<table>
    <tr>
        <td><code>channel name</code></td>
        <td>Name of the channel</td>
        <td></td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Indicates whether the channel is password protected</td>
        <td></td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Indicates whether the channel is temporary, meaning the channel will be deleted after the last person departs</td>
        <td></td>
    </tr>
</table>


### Packet `8`: Context clearing
Informs the client that the context has been cleared.

<table>
    <tr>
        <td><code>int</code></td>
        <td>
            Number indicating what has been cleared.
            <ul>
                <li><code>0</code>: The message list has been cleared.</li>
                <li><code>1</code>: The user list has been cleared.</li>
                <li><code>2</code>: The channel list has been cleared.</li>
                <li><code>3</code>: Both the message and user listing have been cleared.</li>
                <li><code>4</code>: The message, user and channel listing have all been cleared.</li>
            </ul>
        </td>
        <td></td>
    </tr>
    <tr>
        <td><code>channel name</code></td>
        <td>Channel this clear is targeted towards. Ignore packet if this is set and channels are supposedly to be cleared. If this field is empty this packet is intended for the entire context.</td>
        <td>Added in Version 2</td>
    </tr>
</table>


### Packet `9`: Forced disconnect
Informs the client that they have either been banned or kicked from the server.

<table>
    <tr>
        <td><code>bool</code></td>
        <td>
            <ul>
                <li><code>0</code>: The client has been kicked, can reconnect immediately.</li>
                <li><code>1</code>: The client has been banned, can reconnect after the timestamp (documented below) in the next param has expired.</li>
            </ul>
        </td>
        <td></td>
    </tr>
    <tr>
        <td><code>timestamp</code></td>
        <td>Ban expiration time</td>
        <td></td>
    </tr>
</table>


### Packet `10`: User update
Informs that another user's details have been updated.

<table>
    <tr>
        <td><code>int</code></td>
        <td>User ID of the affected user</td>
        <td></td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>New username</td>
        <td></td>
    </tr>
    <tr>
        <td><code>color</code></td>
        <td>New username color</td>
        <td></td>
    </tr>
    <tr>
        <td><code>user permissions</code></td>
        <td>User permissions</td>
        <td></td>
    </tr>
</table>


### Packet `11`: Capability confirmation
Informs the client what capabilities the server understood from the Packet `3`.

Added in Version 2.

<table>
    <tr>
        <td><code>string</code></td>
        <td>A space separated string of accepted capability strings.</td>
        <td></td>
    </tr>
</table>


### Packet `12`: Typing Info
Informs the client that a user is typing.

Added in Version 2.

<table>
    <tr>
        <td><code>channel name</code></td>
        <td>Name of the channel in which the user is typing.</td>
        <td></td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>User ID of the typing user</td>
        <td></td>
    </tr>
    <tr>
        <td><code>timestamp</code></td>
        <td>Time when the user started typing.</td>
        <td></td>
    </tr>
</table>


### Packet `13`: Server Switch
Informs the client that it should connect to a different server.

Added in Version 2.

Packet data tbd.


## Capabilities
Capability strings sent in Client Packet `3` and Server Packet `11`.
If you wish to add your own unofficial capabilities please prefix them with `X_` to avoid conflicts.
Capability strings MUST be all uppercase, MUST only contain alphanumeric characters and underscores and MUST NOT start with a number. (Regular Expression: `[A-Z_][A-Z0-9_]+`)


### `TYPING`: Support for the Typing packets
While Client Packet `4` and Server Packet `12` are core features of Version 2, some people dislike typing indicators and this provides an easy way for servers to skip sending the packets to these users. The client may still inform the server the it is typing though ClientPacket `4`, but it won't receive Server Packet `12`.


### `MCHAN`: Multi-channel support
Allows the client to be present in multiple channels at once. Issuing `/join` will not cause the server to automatically make the user leave the channel.


## Bot Messages
Formatting IDs sent by user -1.

### Informational

#### `say`: Broadcast
Just echo whatever is specified in the first argument.

##### Arguments
 - `string`: Message to be broadcast.


#### `silence`: Silence notice
Informs the client that they've been silenced.


#### `unsil`: Silence revocation notice
Informs the client that their silence has been revoked.


#### `silok`: Silence confirmation
Informs the client that they have successfully silenced another user.

##### Arguments
 - `string`: Username of the user.


#### `usilok`: Silence revocation confirmation
Informs the client that they have successfully revoked another user's silence.

##### Arguments
 - `string`: Username of the user.


#### `flwarn`: Flood protection warning
Informs the client that they are risking getting kicked for flood protection (spam) if they continue sending messages at the same rate.


#### `unban`: Ban revocation confirmation
Informs the client that they have successfully revoked a ban on a user or an IP address.


#### `banlist`: List of banned entities
Provides the client with a list of all banned users and IP addresses.

##### Arguments
 - `string`: HTML with the information on the users with the following format: "<code>&lt;a href="javascript:void(0);" onclick="Chat.SendMessageWrapper('/unban '+ this.innerHTML);"&gt;{0}&lt;/a&gt;</code>" where {0} is the username of the banned user or the banned IP address. The set is separated by "<code>, </code>".


#### `who`: List of online users
Provides the client with a list of users currently online on the server.

##### Arguments
 - `string`: HTML with the information on the users with the following format: "<code>&lt;a href="javascript:void(0);" onclick="UI.InsertChatText(this.innerHTML);"&gt;{0}&lt;/a&gt;</code>" where {0} is the username of a user. The current online user is highlighted with "<code> style="font-weight: bold;"</code>" before the closing &gt; of the opening &lt;a&gt; tag. The set is separated by "<code>, </code>".


#### `whochan`: List of users in a channel.
Provides the client with a list of users currently online in a channel.

##### Arguments
 - `string`: HTML with the information on the users with the following format: "<code>&lt;a href="javascript:void(0);" onclick="UI.InsertChatText(this.innerHTML);"&gt;{0}&lt;/a&gt;</code>" where {0} is the username of a user. The current online user is highlighted with "<code> style="font-weight: bold;"</code>" before the closing &gt; of the opening &lt;a&gt; tag. The set is separated by "<code>, </code>"


#### `join`: User connected
Informs the client that a user just connected to the server.

##### Arguments
 - `string`: Username of the user.


#### `jchan`: User joined channel
Informs the client that a user just joined a channel they're in.

##### Arguments
 - `string`: Username of the user.


#### `leave`: User disconnected
Informs the client that a user just disconnected from the server.

##### Arguments
 - `string`: Username of the user.


#### `lchan`: User left channel
Informs the client that a user just left a channel they're in.


#### `kick`: User has been kicked
Informs the client that another user has just been kicked.

##### Arguments
 - `string`: Username of the user.


#### `flood`: User exceeded flood limit
Informs the client that another user has just been kicked for exceeding the flood protection limit.

##### Arguments
 - `string`: Username of the user.


#### `timeout`: User has timed out
Informs the client that another user has been disconnected from the server automatically.

##### Arguments
 - `string`: Username of the user.


#### `nick`: User has changed their nickname
Informs the client that a user has changed their nickname.

##### Arguments
 - `string`: Previous username of the user.
 - `string`: New username of the user.


#### `crchan`: Channel creation confirmation
Informs the client that the channel they attempted to create has been successfully created.

##### Arguments
 - `string`: Name of the target channel.


#### `delchan`: Channel deletion confirmation
Informs the client that the channel they attempted to delete has been successfully deleted.

##### Arguments
 - `string`: Name of the target channel.


#### `cpwdchan`: Channel password update confirmation
Informs the client that they've successfully changed the password of a channel.


#### `cprivchan`: Channel rank update confirmation
Informs the client that they've successfully changed the minimum required rank to join a channel.


#### `ipaddr`: IP address
Shows the IP address of another user to a user with moderation privileges.

##### Arguments
 - `string`: Name of the target user.
 - `string`: IP address.


### Errors

#### `generr`: Generic Error
Informs the client that Something went Wrong.


#### `nocmd`: Command not found
Informs the client that the command they tried to run does not exist.

##### Arguments
 - `string`: Name of the command.


#### `cmdna`: Command not allowed
Informs the client that they are not allowed to use a command.

##### Arguments
 - `string`: Name of the command.


#### `cmderr`: Command format error
Informs the client that the command they tried to run was incorrectly formatted.

##### Arguments
 - `string`: Name of the command.


#### `usernf`: User not found
Informs the client that the user argument of a command contains a user that is not known by the server.

##### Arguments
 - `string`: Name of the target user.


#### `rankerr`: Rank error
Informs the client that they are not allowed to do something because their ranking is too low.


#### `nameinuse`: Name in use
Informs the the client that the name they attempted to choose is already in use by another user.

##### Arguments
 - `string`: Name that is in use.


#### `whoerr`: User listing error
Informs the client that they do not have access to the channel they tried to query.

##### Arguments
 - `string`: Name of the channel.


#### `kickna`: Kick or ban not allowed
Informs the client that they are not allowed to kick a user.

##### Arguments
 - `string`: Username of the user in question.


#### `notban`: Not banned
Informs the client that the ban they tried to revoke was not in place.

##### Arguments
 - `string`: Username or IP address in question.


#### `nochan`: Channel not found
Informs the client that the channel they tried to join does not exist.

##### Arguments
 - `string`: Name of the channel.


#### `samechan`: Already in channel
Informs the client that they attempted to join a channel they are already in.

##### Arguments
 - `string`: Name of the channel.


#### `ipchan`: Channel join not allowed
Informs the client that they do not have sufficient rank or permissions to join a channel.

##### Arguments
 - `string`: Name of the channel.


#### `nopwchan`: No password provided
Informs the client that they must specify a password to join a channel.

##### Arguments
 - `string`: Name of the channel.


#### `ipwchan`: No password provided
Informs the client that the password they provided to join a channel was invalid.

##### Arguments
 - `string`: Name of the channel.


#### `inchan`: Invalid channel name
Informs the client that the name they tried to give to a channel contains invalid characters.


#### `nischan`: Channel name in use
Informs the client that the name they tried to give to a channel is already used by another channel.

The first argument contains the name of the channel.


#### `ndchan`: Channel deletion error
Informs the client that they are not allowed to delete a channel.

The first argument contains the name of the channel.


#### `namchan`: Channel edit error
Informs the client that they are not allowed to edit a channel.

The first argument contains the name of the channel.


#### `delerr`: Message deletion error
Informs the client that they are not allowed to delete a message.


#### `silerr`: Already silenced
Informs the client that the user they attempted to silence has already been silenced.


#### `usilerr`: Not silenced
Informs the client that the user whose silence they attempted to revoke has not been silenced.


#### `silperr`: Silence permission error
Informs the client that they are not allowed to silence the other user.


#### `usilperr`: Silence revocation permission error
Informs the client that they are not allowed to revoke the silence on the other user.


#### `silself`: Self silence
Informs the client that they are not allowed to silence themselves.


## Commands
Actions sent through messages prefixed with `/`. Arguments are described as `[name]`, optional arguments as `[name?]`. The `.` character is ignored in command names (replaced by nothing).

### `/afk`: Setting status to away
Marks the current user as afk, the first 5 characters from the user string are prefixed uppercase to the current username prefixed by `&amp;lt;` and suffixed by `&amp;gt;_` resulting in a username that looks like `&lt;AWAY&gt;_flash` if `/afk away` is ran by the user `flash`. If no reason is specified "`AFK`" is used.

#### Format
```
/afk [reason?]
```


### `/nick`: Change nickname
Temporarily changes the user's nickname, generally with a prefix such as `~` to avoid name clashing with real users. If the user's original name or no argument at all is specified, the command returns the user's name to its original state without the prefix.

#### Format
```
/nick [name?]
```

#### Responses
 - `cmdna`: User is not allowed to change their own nickname.
 - `nameinuse`: The specified nickname is already in use by another user.
 - `nick`: Username has changed.


### `/msg`: Sending a Private Message
Sends a private message to another user. In implementations of Version 2 or above this should create a private channel that the user can actually join.

#### Format
```
/msg [username] [message]
```

#### Aliases
 - `/whisper`

#### Responses
 - `cmderr`: Missing username and/or message arguments.
 - `usernf`: Target user could not be found by the server.


### `/me`: Describing an action
Sends a message but with flags `11000` instead of the regular `10010`, used to describe an action.

#### Format
```
/me [message]
```

#### Aliases
 - `/action`


### `/who`: Requesting a user list
Requests a list of users either currently online on the server in general or in a channel. If no argument is specified it'll return all users on the server, if a channel is specified it'll return all users in that channel.

#### Format
```
/who [channel?]
```

#### Responses
 - `nochan`: The given channel does not exist.
 - `whoerr`: The user does not have access to the channel.
 - `whochan`: Listing of users in the channel.
 - `who`: Listing of users in the server.


### `/delete`: Deleting a message or channel
Due to an oversight in the original implementation, this command was specified to be both the command for deleting messages and for channels. Fortunately messages always have numeric IDs and channels must start with an alphabetic character. Thus if the argument is entirely numeric this function acts as an alias for `/delmsg`, otherwise `/delchan`.

#### Format
```
/delete [channel name or message id]
```

#### Responses
Inherits the responses of whichever command is forwarded to.


### `/join`: Joining a channel
Switches or joins the current user to a different channel.

In Version 2 with the MCHAN capability a user can be in more than one channel so the behaviour of this command changes to just joining the new channel rather than also immediately leaving previous channel.

#### Format
```
/join [channel] [password?]
```

#### Responses
 - `nochan`: The given channel does not exist.
 - `ipchan`: The client does not have the required rank to enter the given channel.
 - `nopwchan`: A password is required to enter the given channel.
 - `ipwchan`: The password provided was invalid.


### `/leave`: Leaving a channel
Leave a specified channel.

Added in Version 2 through the MCHAN capability. The command should pretend it doesn't exist if MCHAN isn't part of the session's capabilities.

#### Format
```
/leave [channel]
```

#### Responses
 - `nocmd`: The client tried to run this command without specifying the `MCHAN` capability.


### `/create`: Creating a channel
Creates a new channel.

#### Format
```
/create [rank?] [name...]
```

If the first argument is numeric, it is taken as the minimum required rank to join the channel. All further arguments are glued with underscores to create the channel name.

#### Responses
 - `cmdna`: The client is not allowed to create channels.
 - `cmderr`: The command is formatted incorrectly.
 - `rankerr`: The specified rank is higher than the client's own rank.
 - `inchan`: The given channel name contains invalid characters.
 - `nischan`: A channel with this name already exists.
 - `crchan`: The channel has been created successfully.


### `/delchan`: Deleting a channel
Deletes an existing channel.

#### Format
```
/delchan [name]
```

#### Responses
 - `cmderr`: The command is formatted incorrectly.
 - `nochan`: No channel exists with this name.
 - `ndchan`: The client is not allowed to delete this channel.
 - `delchan`: The target channel has been deleted.


### `/password`: Update channel password
Changes the password for a channel. Removes the password if no argument is given.

#### Format
```
/password [password?]
```

#### Aliases
 - `/pwd`

#### Responses
 - `cmdna`: The client is not allowed to change the password for this channel.
 - `cpwdchan`: The password of the channel has been successfully updated.


### `/rank`: Update channel minimum rank
Changes what user rank is required to enter a channel.

#### Format
```
/rank [rank]
```

#### Aliases
 - `/privilege`
 - `/priv`

#### Responses
 - `cmdna`: The client is not allowed to change the rank of the target channel.
 - `rankerr`: Missing rank argument or the given rank is higher than the client's own rank.
 - `cprivchan`: The minimum rank of the channel has been successfully updated.


### `/say`: Broadcast a message
Broadcasts a message as the server/chatbot to all users in all channels.

#### Format
```
/say [message]
```

#### Responses
 - `cmdna`: The client is not allowed to broadcast messages.
 - `say`: The broadcasted message.


### `/delmsg`: Deleting a message
Deletes a given message.

#### Format
```
/delmsg [message id]
```

#### Responses
 - `cmdna`: The client is not allowed to delete messages.
 - `cmderr`: The given message ID was invalid.
 - `delerr`: The target message does not exist or the client is not allowed to delete this message.


### `/kick`: Kick a user
Kicks a user from the serer. If not time is specified, then kick expires immediately.

#### Format
```
/kick [username] [time?]
```

#### Responses
 - `cmdna`: The client is not allowed to kick others.
 - `usernf`: The target user could not be found on the server.
 - `kickna`: The client is trying to kick someone who they are not allowed to kick, or someone that is currently banned.
 - `cmderr`: The provided time is invalid.


### `/ban`: Bans a user
Bans a user and their IP addresses from the server. If no time is specified the ban will never expire.

#### Format
```
/ban [user] [time?]
```

#### Responses
 - `cmdna`: The client is not allowed to kick others.
 - `usernf`: The target user could not be found on the server.
 - `kickna`: The client is trying to kick someone who they are not allowed to kick, or someone that is currently banned.
 - `cmderr`: The provided time is invalid.


### `/pardon`: Revokes a user ban
Revokes a ban currently placed on a user.

#### Format
```
/pardon [user]
```

### Aliases
 - `/unban`

#### Responses
 - `cmdna`: The client is not allowed to revoke user bans.
 - `notban`: The target user is not banned.
 - `unban`: The ban on the target user has been successfully revoked.


### `/pardonip`: Revokes an IP address ban
Revokes a ban currently placed on an IP address.

#### Format
```
/pardonip [address]
```

#### Aliases
 - `/unbanip`

#### Responses
 - `cmdna`: The client is not allowed to revoke IP bans.
 - `notban`: The target address is not banned.
 - `unban`: The ban on the target address has been successfully revoked.


### `/bans`: List of bans
Retrieves a list of banned users and IP addresses.

#### Format
```
/bans
```

#### Aliases
 - `/banned`

#### Responses
 - `cmdna`: Not allowed to view banned users and IP addresses.
 - `banlist`: The list of banned users and IP addresses.


### `/silence`: Silence a user
Silences a user. If the time argument is not specified, the silence is indefinite.

#### Format
```
/silence [username] [time?]
```

#### Responses
 - `cmdna`: The client is not allowed to silence users.
 - `usernf`: The target user could not be found on the server.
 - `silself`: The client tried to silence themselves.
 - `silperr`: The target user has a higher rank that the client.
 - `silerr`: The target user is already silenced.
 - `cmderr`: The time argument is formatted incorrectly.
 - `silence`: Informs the target user that they have been silenced.
 - `silok`: The target has been successfully silenced.


### `/unsilence`: Revokes a user silence
Revokes a user's silenced status.

#### Format
```
/unsilence [username]
```

#### Responses
 - `cmdna`: The client is not allowed to revoke silences.
 - `usernf`: The target user could not be found.
 - `usilperr`: The target user has a higher rank than the client.
 - `usilerr`: The target user isn't silenced.
 - `unsil`: Informs the target user that their silenced status has been revoked.
 - `usilok`: The silenced status placed on the target has been successfully revoked.


### `/ip`: Retrieve IP addresses
Retrieves a user's IP addresses. If the user has multiple connections, multiple `ipaddr` responses may be sent.

#### Format
```
/ip [username]
```

#### Aliases
 - `whois`

#### Responses
 - `cmdna`: The client is not allowed to view IP addresses.
 - `usernf`: The target user is not connected to the server.
 - `ipaddr`: (One of) The target user's IP address(es).
