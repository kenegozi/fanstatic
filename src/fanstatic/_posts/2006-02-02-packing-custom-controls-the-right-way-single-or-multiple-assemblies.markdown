---
{layout: "post",
title:  "Packing custom controls the right way - single or multiple assemblies?",
comments: true,
tags: ["architecture"]
}
---


We're developing a new web application at our department with rich client interface,and as part of the process we need to develop a few UI custom controls, that can and will be reusable in future projects.

I am indecision about two approaches:1. Packing all the UI web controls into a single assembly (let's say SQLink.Web.UI), as MS did with their System.Web (which include all the UI namespaces, and all other web namespaces, too), and also with the Microsoft.Web.UI.WebControls.dll2. Pack every single control into adifferent assembly.

Why 1? it's somewhat easier to maintain (a single project on VSS), it's following the path of MS (this isn't **always** the best thing to do, but it often is), and it's easier to deploy.

Why 2? (a)It's easier for two developers to work on different controls without "fighting" over source control privileges, and (b) if the assembly holds valuable controls not needed to a specific application, and the application is handed to a client who isn't licensed to use those controls, he still can lay his hands on it if we had packed all our controls into a single dll.

After consultation with my colleague [Oren Ellenbogen](http://www.lnbogen.com), we came to conclusion that approach 1 is better, and that the downsides of it can be solved by (a) working properly (by means of not allowing the developers to keep project wide objects checked-out), and (b) relying on the legal discipline of our clients, not to use our application dlls in their or in third party solutions, without our explicit permission.

back to coding ...



