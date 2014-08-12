---
{layout: "post",
title:  "VS 2005 adding web project's pdb files to vss - why is that and how to fix it",
comments: true,
tags: ["asp-net-2-0","visual-studio","scm"]
}
---


Well, Here's my first hopfully useful post in my blog.

The problem:

pdb files of projects and assemblies being referenced by a web application, somtimes become "source controlled" when using VS 2005 and VSS. This is leading to the pdbs and even dlls to become readonly, and therefore the break of a succesful post-build events, such as copying newly compiled dlls to the web application's Bin directory, hence unexpected behavior of the application.

The "Why the heck this is happening":

Using VS 2005 to develop ASP.NET 2.0 applications is somewhat different than in the good old VS 2003.

The main difference is caused by the missing project file.

As opposed to VS 2003, which compiled the cs (and vb) files of a web project using csc.exe (and vbc.exe), as with regular (not web) projects, things are different with VS 2005 web projects.

In order to allow on the fly compile of source files (codebehinds and App_Code files), and on the other hand, to allow a precompilation ofthe whole project (including aspx, ascxand other "non code" files), the framework has provided us with a special compiler, named aspnet_compiler.exe

allowing a full "automated" compiled outside of any IDE, requires disrelying on an IDE specific file, such as the csproj/vbproj files, therefore, everything in (and under) the application's directory considered to be a part of the project.

In VS 2003 the compiler knew about referenced dlls and projects from the csproj/vbproj file. Now referenced dlls are known throuh a .refresh file in the Bin directory, that points to the location of the dll, and referenced project files are written to a special section in the web.config.

Controlling the source manually using VSS interface is possible, yet you have to manually remember not to check in items from the Bin directory, except the .refresh files.

The VSS integretion in VS 2005 is trying to outsmart the problem. it assumes that if a .refrsh file exists in the Bin directory, then the file it references is not to be source controlled. For example: if the Bin directory contains a file names MyAssembly.dll.refresh, then if it also contains a file names MyAssembly.dll (and it will, after a succesfull build), it won't treat it as source controlled.

It also understands that if a file is there, due to a build action, it is also marked so it won't besource controlled.

But, if you build you solution in Debug configuration,now you have pdb files in the Bin directory. let's say that you do a Release Build, that does not create and copy pdb files. now the existing pdb files in the Bin directory are not marked, and the VSS integration will sense that there are new files in the Bin, that have no .refresh file, andwas not created during the last Build event, so it will mark them as Check InPending files !!!

The "Gosh it's Ugly solution":

You need to:1.Rebuild the solution in Debug configuration2. Close and Reopen the project.

If you've already accidently Checked In the pdb files, you need to:1. Manually delete them from the VSS.2. Manually set their readonly attribute to false.3.Rebuild the solution in Debug configuration4. Close and Reopen the project.

The place where i've found this solution: [http://forums.microsoft.com/MSDN/ShowPost.aspx?PostID=106422&amp;SiteID=1](http://forums.microsoft.com/MSDN/ShowPost.aspx?PostID=106422&amp;SiteID=1)

So until next time, keep on with the good coding thing you've beendoing while you've start reading this post.

Ken

