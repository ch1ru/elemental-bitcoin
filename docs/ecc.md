[/Intro](/index.md)|[/Install](/install.md)|[/keys](/keys.md)|[/Crypto](ecc.md)|[/Wallet](wallet.md)|[/Transactions](transactions.md)|[/Script](script.md)|[/Blocks](blocks.md)|[/Mining](/mining.md)|[/SPV](spv.md)|[/Segwit](segwit.md)

# Elliptic Curve Cryptography

This chapter will be about the maths behind bitcoin keys and signing. Feel free to skip this chapter, it's just for extra insight into cryptographic operations within the bitcoin protocol. 

## What is an elliptic curve? 

An elliptic curve is any curve with the general formula:

y^2 = ax^3 + ax + b

An interesting property of elliptic curves is that they display intuitive properties we see in standard maths, namely: associativity and commutativity.

If an elliptic curve is intersected by a straight line, unless is some exceptions, it will intersect at 3 points, such that: 

Point A + point B = point C reflected on the x axis. 

Here's an example:

![elliptic curve](/assets/ellipticcurve1.png)

P + Q = R.

We can also intersect at the tangent (P) then use differentiation to find the next intersection (-2P) then reflect over the x-axis to get 2P:

![elliptic curve tangent](/assets/ellipticcurve2.jpg)

**The maths for the last diagram**

We are trying to find the point of the next intersect (x3, y3) or -R in the diagram.

Equation of curve: y^2 = x^3 + ax + b
Find differentiation equation: 
dy 2y = 3x^2 + a dx
dy/dx 2y = 3x^2 + a
dy/dx = (3x^2 + a)/2y

Gradient, m = (3(p.x)^2 + a) / 2(p.y)

Using Fermat's little theorem, we know that: x3 = m^2 - 2(p.x), So we can calculate the x-coordinate x3
We also know that y3 = (m * (p.x - x3)) - p.y, so we now know -R!

We can reflect over the x-axis so negate y3.

## So how are elliptic curve keys made?



[/Intro](/index.md)|[/Install](/install.md)|[/keys](/keys.md)|[/Crypto](ecc.md)|[/Wallet](wallet.md)|[/Transactions](transactions.md)|[/Script](script.md)|[/Blocks](blocks.md)|[/Mining](/mining.md)|[/SPV](spv.md)|[/Segwit](segwit.md)
