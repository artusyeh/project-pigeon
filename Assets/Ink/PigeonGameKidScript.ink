->ParkKidStart

=== ParkKidStart ===
#speaker Little Kid
Sunglasses...

#speaker Little Kid
Sunglasses...

#speaker Little Kid
Oh no...

+[Ask What's Wrong] ->whatsWrong
+[Ignore] ->leave1

=== whatsWrong ===
#speaker You
What's wrong?

#speaker Little Kid
I lost my sunglasses!

#speaker Little Kid
They used to go right HERE.

#speaker Narrator
He points to his cheeks.

#speaker Little Kid
I need to find it to be super duper cool again.

#speaker Little Kid
Like this.

#speaker Narrator
He T-poses in front of you.

+[Offer to Help] ->helpKid
+[Wish Him Luck] ->leave2

=== helpKid ===
#speaker You
I'll help you look for it.

#speaker Little Kid
Thanks! You can have my propeller hat in return.

#speaker Little Kid
I too cool for childish things like that.

#speaker You
Where do you think you lost your sunglasses?

#speaker Little Kid
Maybe in the Sheep place! Where I had picnic with mommy.

#speaker Little Kid
Let me know if you find it, okay?

#speaker Little Kid
I'm gonna keep searching.

#speaker Narrator
You watch as he proceeds to go on the swings.
->END

=== leave1 ===
You decide to not involve yourself.

->END

=== leave2 ===
#speaker You
Well, I wish you luck!

->END
