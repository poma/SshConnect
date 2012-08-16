AwsSsh 1.0 by Roman Semenov
===========================

A nice windows client that lets you quickly SSH to your Amazon EC2 instances. Has built-in search that helps a lot when you have many instances.

![Screenshot](https://raw.github.com/poma/AwsSsh/master/Screenshots/Screenshot1.png)

You can download compiled version from [downloads page](https://github.com/poma/AwsSsh/downloads)


Requirements
------------
Requires .NET Framework 4.0

Before using it you need to edit AwsSsh.exe.config.

Fields required to normal operation are:

* Your amazon access and secret keys (grab them [here](https://portal.aws.amazon.com/gp/aws/securityCredentials))
* Path to putty.exe (you can download it from [official site](http://www.chiark.greenend.org.uk/~sgtatham/putty/download.html))
* Path to your ssh private key (.ppk)
And don't forget to configure your region URL if it is not default (N. Virginia)



Enjoy.