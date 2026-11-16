VAR badgeQuest = false
VAR badgeAquired = false

{
- not badgeQuest && not badgeAquired:
    ->PoliceStart
- badgeQuest && not badgeAquired:
    ->PoliceWait
- else:
    ->BadgeFound
}

=== BadgeFound ===
#speaker Brian the NYPD Officer
Nice job, pigeon!

#speaker Brian the NYPD Officer
Now I won't have to report my badge missing.

#speaker Brian the NYPD Officer
Here.

#speaker Brian the NYPD Officer
A sticker for your hard work.

#speaker Narrator
Brian gives you a sticker that reads "Outstanding Citizen."

#speaker Narrator
It barely sticks to your feathers.

#speaker Brian the NYPD Officer
Thanks again!

#speaker Narrator
By the time Brian left, the wind had already blown the sticker away.

->END

=== PoliceWait ===
#speaker Brian the NYPD Officer
Find anything, pal?

#speaker Twiggy
Not yet...

#speaker Brian the NYPD Officer
Don't worry.

#speaker Brian the NYPD Officer
Just let me know when you find it.

->END

=== PoliceStart ===
#speaker Brian the NYPD Officer
How's your day, pal?

+[Good] ->goodDay
+[Bad] ->badDay

=== goodDay ===
#speaker Twiggy
It's been good!

#speaker Twiggy
You?

#speaker Brian the NYPD Officer
I'm glad to hear that!

#speaker Brian the NYPD Officer
My day hasn't been going so well.

#speaker Brian the NYPD Officer
I lost my police badge...

+[I can help you find it!] ->helpOut
+[I hope you find it soon.] ->dontHelp

=== badDay ===
#speaker Twiggy
It's hasn't been great.

#speaker Brian the NYPD Officer
Sorry to hear that.

#speaker Twiggy
What about you?

#speaker Brian the NYPD Officer
Not much better

#speaker Brian the NYPD Officer
My day hasn't been going so well either.

#speaker Brian the NYPD Officer
I lost my police badge...

+[I can help you find it!] ->helpOut
+[I hope you find it soon.] ->dontHelp

=== helpOut ===
#speaker Twiggy
I can help you find it!

#speaker Brian the NYPD Officer
That'd be great.

#speaker Brian the NYPD Officer
I must have lost it while doing rounds.

#speaker Brian the NYPD Officer
Just keep an eye out for me, okay?

#speaker Brian the NYPD Officer
I'll stay at my post in the meantime.
->END

=== dontHelp ===
#speaker Twiggy
I hope you find it soon.

#speaker Brian the NYPD Officer
Thanks, pal.

->END

