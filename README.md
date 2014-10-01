# Welcome to Minimods! 

**«A Minimod is a single-file distribution of a specific reusable C# code fragment.»**

> and, at this github site, a collection of some Minimods from the authors.

### Current Minimods
Find all currently published Minimods on Nuget Gallery: http://bit.ly/nuget-minimod

### Our Intention

We love being productive. And there are tons of generic problems that we solve far too often; well, or we copy-paste them. Sometimes we package them into *.Common or *.Utils, and put them in our toolbox. But theese are a nightmare; read: [1][1] [2][2]

Minmods are a pragmatic way somewhere between copy-paste and *real* libraries.

### Rules

A Minimod must

 * must consist of one file,
 * be single-purpose,
 * be in it’s own namespace,
 * expose dependies to frameworks outside .NET BCL or other Minimods in its name,
 * be separately versioned. 

We suggest to release Minimods under the Terms of the Apache License 2.0.

### Examples

*FlattenerMinimod* is good, *LinqExtensions* (containing arbitrary code, among them a 'IEnumerable.Flatten<T>(...)' ist bad

*more to come...*

__
original blog post introducing Minimods:
http://startbigthinksmall.wordpress.com/2011/07/05/reuse-reuse-reuse-do-we-need-utility-libraries-if-not-whats-next-minimods/


[1]: http://startbigthinksmall.wordpress.com/2011/07/05/reuse-reuse-reuse-do-we-need-utility-libraries-if-not-whats-next-minimods/
[2]: http://ayende.com/blog/3986/let-us-burn-all-those-pesky-util-common-libraries