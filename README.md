SshConnect 2.0
==========

A nice windows tool that lets you quickly SSH to your Amazon EC2 or Chef-managed instances using Putty. Has built-in search that helps a lot when you have many instances.

![Screenshot](https://cloud.githubusercontent.com/assets/2109710/3471027/4fa72c6c-02bf-11e4-9589-dd05895a12d7.png)

Download
------------
You can download compiled version from [releases page](https://github.com/poma/SshConnect/releases)


Requirements
------------

For normal operation you will need:

* [.NET Framework 4.0](http://www.microsoft.com/en-us/download/details.aspx?id=17851)
* Your Amazon Web Services access and secret keys (grab them [here](https://portal.aws.amazon.com/gp/aws/securityCredentials))
* Putty.exe (you can download it from [official site](http://www.chiark.greenend.org.uk/~sgtatham/putty/download.html))
* Your private ssh key (.ppk). You can create it from .pem using [PuttyGen](http://www.chiark.greenend.org.uk/~sgtatham/putty/download.html)

And don't forget to configure your AWS region URL if it is not default (N. Virginia)

Enjoy.