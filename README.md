# Maat

I don't even really know what this is any longer. It started as a means to learn a little bit about EVent Sourcing, and CQRS. Then I started a yak-shave
with Enbilulu to learn a little bit about event streaming because Kafka seemed way too complex for what I needed. Hilarious, writing my own event streamer
to avoid complexity.

Somewhere along the lines this got caught up in my growing distaste for Twitter and my low-key obsession with the Indieweb. I *have* a micropub service
I can use - Micro.blog. I even have an existing micropub endpoint of my own to do Twitter and Mastadon posting. But sure, let's create another one.

## TODO
* ~support photo property in json create~
* ~Support photo property in json update~
* ~Support photo property in form create~
* ~Re-implement file upload via form create~
* ~Review category support in update~
* ~Review post-status support in update~
* ~IndieAuth endpoints~
* A web app client

## Try Not To Do
* Don't yak shave a microformat parser. Try and be content with the minimally viable parser.
* Don't yak shave a library for urls, for canonicalising not for merging absolute and relative urls. The minimally viable extension functions are enough.