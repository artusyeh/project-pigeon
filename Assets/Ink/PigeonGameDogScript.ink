VAR ballQuest = false
VAR ballAquired = false

{
- not ballQuest && not ballAquired:
    ->startDog
- ballQuest && not ballAquired:
    ->dogWait
- else:
    ->ballFound
}

=== dogWait ===
#speaker Corgi
How much longer... woof...

#speaker Corgi
I'm dying over here!

#speaker Twiggy
How come your owner doesn't play with you?

#speaker Corgi
She's naping in the grass.

#speaker Corgi
So, I have to take advantage of this freedom!

#speaker Twiggy
I see. I will let you know if I find anything.
->END

=== ballFound ===
#speaker Corgi
IS THAT A TENNIS BALL?!

#speaker Corgi
I LOVE TENNIS BALLS.

#speaker Corgi
WOOF, WOOF!

#speaker Twiggy
You nudge the ball at the corgi.

#speaker Corgi
YAYYYYYY!

#speaker Corgi
BALL, BALL, BALL, BALL!

#speaker Narrator
He runs after the ball which is rolling away.

#speaker Narrator
Then he turns around and yells:

#speaker Corgi
THANKS FOR THE HELP!

#speaker Narrator
He wags his tail the whole time, until he is out of view.

->END

=== startDog ===
#speaker Corgi
Woof! Woof!

#speaker Corgi
Woof...

#speaker Twiggy
Why do you keep barking?

#speaker Corgi
I'm bored.

#speaker Corgi
Woof, woof.

+[\[1\] I'll find you something fun.]->help
+[\[2\] Maybe you should take a nap]->leave

=== help ===
~ ballQuest = true
#speaker Twiggy
I'll find you something fun.

#speaker Corgi
Thanks, bro.

#speaker Corgi
Woof.

->END

=== leave ===
#speaker Twiggy
Maybe you should take a nap...

#speaker Corgi
Maybe, bro.

#speaker Corgi
Woof.

->END
