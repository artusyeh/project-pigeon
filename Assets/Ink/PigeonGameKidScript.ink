VAR sunglassesQuest = false
VAR sunglassesAquired = false

{
- not sunglassesQuest && not sunglassesAquired:
    ->ParkKidStart
- sunglassesQuest && not sunglassesAquired:
    ->kid_waiting
- else:
    ->sunglasses_found
}

=== kid_waiting ===
#speaker Little Kid
Don't just stand around!

#speaker Little Kid
The sunglasses won't find themselves, you know!

#speaker Little Kid
Hmph.

->END

=== sunglasses_found ===
#speaker Little Kid
#expression kid_happy
You found my sunglasses!

#speaker Little Kid
Hooray!

#speaker Little Kid
Thank you!

#speaker Little Kid
#expression kid_bald
Here's my propeller hat.

#speaker Little Kid
Even though it's super duper uncool now, uh...

#speaker Little Kid
You have to take good care of it.

#speaker Little Kid
Okay, bye now.

#speaker Little Kid
I'm gonna go be super duper cool with the other kids.

-> END

=== ParkKidStart ===
#speaker Little Kid
#expression kid_sad
Sunglasses...

#speaker Little Kid
Sunglasses...

#speaker Little Kid
Oh no...

+[\[1\] Ask What's Wrong] ->whatsWrong
+[\[2\] Ignore] ->leave1

=== whatsWrong ===
#speaker Twiggy
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

+[\[1\] Offer to Help] ->helpKid
+[\[2\] Wish Him Luck] ->leave2

=== helpKid ===
#speaker Twiggy
I'll help you look for it.

#speaker Little Kid
#expression kid_happy
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
#speaker Twiggy
Well, I wish you luck!

#speaker Little Kid
#expression kid_sad
Okay, I guess...

->END
