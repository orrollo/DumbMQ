# DumbMQ
yet another message transport system. absolutely dumb and simple.

i was thinked a lot about message transport systems. i looked to rabbitmq, dotnetmq, nsq... they are cool. but very complex for the most of mine tasks. i need a really simple, dumb to use system. and i want to build something new in my expirence. i want to try make own mq.

it is.

main ideas

there are two application types here: clients and servers. clients are services or applications, servers are transport engine.

- every client connected to a single server.
- every client is named.
- name of client is unique for a client type (functional interface). same client names mean what it's are instances of same type.

- can be several servers in system.
- every server are named.
- server names are unique.
- every server know one or more other servers.
- every server contain own client part for internal needs.

- all data translated between clients are messages


