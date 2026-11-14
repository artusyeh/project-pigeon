-> startDog

=== startDog ===
#speaker Corgi
Woof! Woof!

#speaker Corgi
Woof...

#speaker You
Why do you keep barking?

#speaker Corgi
I'm bored.

#speaker
Woof, woof.

+[I'll find you something fun.]->help
+[Maybe you should take a nap]->leave

=== help ===
#speaker You
I'll find you something fun.

#speaker Corgi
Thanks, bro.

#speaker Corgi
Woof.

->END

=== leave ===
#speaker You
Maybe you should take a nap...

#speaker Corgi
Maybe, bro.

#speaker Corgi
Woof.

->END
