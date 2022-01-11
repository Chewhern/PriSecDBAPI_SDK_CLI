# PriSecDBAPI_CRUD_SDK
This is the second part of 3 parts of client side SDK for PriSecDBAPI. This SDK was responsible in mainly providing functionality of **CRUD** which consists only of **Normal_Insert,Special_Insert,Select,DecryptRetrievedValues,Update,Delete**.

This SDK might fit in a low code environment if you are using C#, I have tried my best in trying to make this SDK as secure as possible. This SDK has **easier access** to the
features or functionalities but it **does not mean it's easy to use or has high familiarity**.

Other SDK like **PriSecDBAPI_Cryptography_SDK** will have their own different purposes. By separating **CRUD** and **SC** into different libaries
or dll, people can choose whether they would want the optional **Cryptography_SDK**. Not to mention, the chances of cyber attacks such as **Log4J** could be reduced as the
libraries were added up instead of bundling all libraries into a single library.
