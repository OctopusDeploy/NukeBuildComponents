# NukeBuildComponents

This repo is an experiment with using [Nuke Build Components](https://nuke.build/docs/sharing/build-components/), as a way of de-duplicating code across builds.

## Deprecated

In the end, we found using build components ended up hiding things, and making things too magical. It made it hard for new people to look at the code and see "this is what it's doing".

Instead, we recommend having your nuke build code directly in your project. If you want to abstract things out, it's worth abstracting at a similar level to the `DotNetTasks`, so that individual actions are extracted, not the logic of steps that need to happen to build / publish your project.
