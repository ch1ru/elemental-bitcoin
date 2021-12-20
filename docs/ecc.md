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

![elliptic curve tangent](/assets/ellipticcurve2.jpeg)

**The maths for the last diagram**

We are trying to find the point of the next intersect (x3, y3) or -R in the diagram.
```
Equation of curve: y^2 = x^3 + ax + b

Find differentiation equation: 

dy 2y = 3x^2 + a dx

dy/dx 2y = 3x^2 + a

dy/dx = (3x^2 + a)/2y

Gradient, m = (3(p.x)^2 + a) / 2(p.y)

Using Fermat's little theorem, we know that: x3 = m^2 - 2(p.x), So we can calculate the x-coordinate x3

We also know that y3 = (m * (p.x - x3)) - p.y, so we now know -R!

We can reflect over the x-axis so negate y3.
```

## So how are elliptic curve keys made?

We Mentioned previously that a public key is a point on an elliptic curve. The process of generating it first involves randomly generating a 256-bit value, which is the private key. You can think of this as a scalar, or point multiplyer. Technically we can't do multiplication on the curve (how do we find the product of 2 points?) but we can do point addition many times. Finding 2P (The process of drawing a tangent at P like we explained above) is the same as P + P. 5P is P + P + P + P + P etc. Easy right? So the public key is the multiplication of a known point, G, and the 256-bit random number. 

## How big is the private key?

256 bits can be stored in a 32-byte hex string. Despite this, 2^256 is insanely huge. To put it into perspective, It is thought that the milky way may contain around 10^67 atoms. However this is only a fraction of the number of possible elliptic curve private keys! Clearly, in a crypto system which calculates the public key by hopping around a curve the number of times there are atoms in thousands of galaxies is not very useful to our current hardware limits! It turns out there is a nifty trick called binary expansion which can much reduce the problem and make it computationally trivial to solve.

## Elliptic curves within finite fields

All the maths we have covered so far is all useful and correct(ish), however it's not a full cryptosystem yet. The curve is actually bounded by a finite field. All the operations we have covered do indeed take place, but within a prime field. The equation that bitcoin uses called Secp256k1 curve equation is actually: y^2 = x^3 + 7 mod P, where P is a very large prime (2^256 - 2^32 - 977 to be exact). 

This gives the curve some interesting properties. Visually, it no longer looks like a curve but a series of scattered points.

![Elliptic curve mod](/assets/ellipticcurve3.png)

Interestingly, all the mathmatic identities and properties like associativity and commutavity remain unchanged. We can calculate everything as we did before but for every basic operation, it also requires a modular operation of the large prime (% P).

## Finding the last missing group axiom

A group in a cryptosystem requires at least these 4 properties:

- Associativity - (A + B) + C = A + (B + C). This isn't immediately obvious in elliptic curves, but try visualising P + Q 
- Closure
- Inverse element
- Identity element

In addition to these there is commutativity, which we mentioned previously. A group is commutative if x * y = y * x. This is easy to prove in elliptic curves; a line intersecting P and Q will produce the same result no mater the order.

A group is called cyclic if there is an element, G, which spans all elements in the group (g^1, g^2, g^3 ...etc). In elliptic curves, this is also true since the base point G of the curve will span all points in the additive group (G, 2G, 3G % P).

All these properties come organically within the elliptic curve cryptosystem except one: the identity element. Which point can we add to another to get its identity? There is none. To combat this, there is an imaginary point called the point at infinity which, when added to a point, will get its identity. We can visualise this by drawing a vertical line down the elliptic curve. The line will intersect at exactly 2 places. We also say it intersects at a third: the point at infinity. Following the rules we have established so far, when we add the point at infinity to P, the line intersects at one other place, at -P. Reflecting this back across the x-axis gives us P. Thus we have found our missing axiom! (albeit a slightly convuluted and perhaps uintuitive sense).





[/Intro](/index.md)|[/Install](/install.md)|[/keys](/keys.md)|[/Crypto](ecc.md)|[/Wallet](wallet.md)|[/Transactions](transactions.md)|[/Script](script.md)|[/Blocks](blocks.md)|[/Mining](/mining.md)|[/SPV](spv.md)|[/Segwit](segwit.md)
