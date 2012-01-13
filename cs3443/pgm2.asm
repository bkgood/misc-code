# William Good
# Program 2
# 32 bit ALU
# Another (far shorter) method to implement the ripple carry multi-bit
# adder is to use a loop and a simple counter (incremented by 1 each
# iteration). However, this means our adder is actually dependant on
# another adder which leads to a rather undesirable circular definition
# -- in that case, one must have an adder to implement the adder.
# This was how I originally implemented the Add code, using a MIPS add
# instruction to perform the increment, but this seemed incorrect in the
# true spirit of the assignment. As such, I chose to manually iterate
# through all 32 bits, which means this file is just short of 500 lines.
# For reference, I did include the original loop code, it's been commented
# out around lines 73-85. To use it, simply uncomment those lines and delete
# lines 86-341 (inclusive). It's a bit of a cheat, but far more elegant.
# William Good 04-20-2010

.data
# these are the instructions represented by opcodes, in order
instructions: .word And, Or, Add, Sub, Slt
OverflowNotice: .asciiz "\nOverflow bit: "
.text
.globl main

# performs v0 = a0 & a1
And:
	and $v0, $a0, $a1
	jr $ra

# performs v0 = a0 | a1
Or:
	or $v0, $a0, $a1
	jr $ra

# a: a0, b: a1, cin: a2
# sum: v0 cout: v1
# adds first bit (LSB) of a and b (implementation of a full adder)
BitAdd:
	# first, trim $a0 and $a1 to one bit:
	andi $a0, $a0, 1
	andi $a1, $a1, 1
	# sum = (a ^ b) ^ cin
	xor $v0, $a0, $a1					# a ^ b
	xor $v0, $v0, $a2					# cin ^ (a ^ b)
	# cout = (a & b) | (cin & (a ^ b))
	and $t0, $a0, $a1					# a & b
	xor $v1, $a0, $a1					# a ^ b
	and $v1, $v1, $a2					# (a ^ b) & cin
	or  $v1, $t0, $v1					# (a & b) | ((a ^ b) & cin)
	jr $ra
# end of BitAdd

# adds a and b
# takes a in $a0 and b in $a1
# returns sum in $v1 and to be overflow bit in $v1
Add:
	addi $sp, $sp, -24			# get 6 words on stack
	sw $ra, 0($sp)				# store return address
	sw $s0, 4($sp)				# s0 = a
	sw $s1, 8($sp)				# s1 = b
	sw $s2, 12($sp)				# s2 = sum
	sw $s3, 16($sp)				# s3 = counter, used in shifts
	sw $s4, 20($sp)				# s4 = sign bit store, for overflow
	move $s0, $a0				# save a
	move $s1, $a1				# save b
	srl $s4, $s0, 31			# put sign bit of a in lsb of s4
	srl $t0, $s1, 31			# shift sign bit of b all the way to right
	sll $t0, $t0, 1				# shift back 1 (prev shift cleared other bits)
	or $s4, $s4, $t0			# combine sign bits of a and b
	li $s2, 0					# load the sum with 0
	li $s3, 0					# load cout with 0
	move $a2, $zero				# initialize cin to the adder to 0
#AddNextBit:
#	move $a0, $s0				# a0 = a for bitadd
#	move $a1, $s1				# a1 = b for bitadd
#	jal BitAdd					# run the adder on the first bits of a and b
#	sllv $v0, $v0, $s3			# shift the sum the necessary bits left
#	or $s2, $s2, $v0			# turn on the bit of the final sum if necessary
#	move $a2, $v1				# copy the cout to cin for the next adder call
#	srl $s0, $s0, 1				# shift a right one to add the next bit
#	srl $s1, $s1, 1				# shift b right one to add the next bit
#	addi $s3, $s3, 1			# incr to the shift count, for shifting the sum
#	li $t0, 32					# load 32, if shift is 32 bits then we're done
#	beq $s3, $t0, AddEnd		# stop the iteration in this case
#	j AddNextBit				# jump to beginning of the routine for next bit
	move $a0, $s0				# a0 = a for bitadd
	move $a1, $s1				# a1 = b for bitadd
	jal BitAdd					# run the adder on the first bits of a and b
	sll $v0, $v0, 0				# shift the sum the necessary bits left
	or $s2, $s2, $v0			# turn on the bit of the final sum if necessary
	move $a2, $v1				# copy the cout to cin for the next adder call
	srl $s0, $s0, 1				# shift a right one to add the next bit
	srl $s1, $s1, 1				# shift b right one to add the next bit
	move $a0, $s0				# a0 = a for bitadd
	move $a1, $s1				# a1 = b for bitadd
	jal BitAdd					# run the adder on the first bits of a and b
	sll $v0, $v0, 1				# shift the sum the necessary bits left
	or $s2, $s2, $v0			# turn on the bit of the final sum if necessary
	move $a2, $v1				# copy the cout to cin for the next adder call
	srl $s0, $s0, 1				# shift a right one to add the next bit
	srl $s1, $s1, 1				# shift b right one to add the next bit
	move $a0, $s0				# a0 = a for bitadd
	move $a1, $s1				# a1 = b for bitadd
	jal BitAdd					# run the adder on the first bits of a and b
	sll $v0, $v0, 2				# shift the sum the necessary bits left
	or $s2, $s2, $v0			# turn on the bit of the final sum if necessary
	move $a2, $v1				# copy the cout to cin for the next adder call
	srl $s0, $s0, 1				# shift a right one to add the next bit
	srl $s1, $s1, 1				# shift b right one to add the next bit
	move $a0, $s0				# a0 = a for bitadd
	move $a1, $s1				# a1 = b for bitadd
	jal BitAdd					# run the adder on the first bits of a and b
	sll $v0, $v0, 3				# shift the sum the necessary bits left
	or $s2, $s2, $v0			# turn on the bit of the final sum if necessary
	move $a2, $v1				# copy the cout to cin for the next adder call
	srl $s0, $s0, 1				# shift a right one to add the next bit
	srl $s1, $s1, 1				# shift b right one to add the next bit
	move $a0, $s0				# a0 = a for bitadd
	move $a1, $s1				# a1 = b for bitadd
	jal BitAdd					# run the adder on the first bits of a and b
	sll $v0, $v0, 4				# shift the sum the necessary bits left
	or $s2, $s2, $v0			# turn on the bit of the final sum if necessary
	move $a2, $v1				# copy the cout to cin for the next adder call
	srl $s0, $s0, 1				# shift a right one to add the next bit
	srl $s1, $s1, 1				# shift b right one to add the next bit
	move $a0, $s0				# a0 = a for bitadd
	move $a1, $s1				# a1 = b for bitadd
	jal BitAdd					# run the adder on the first bits of a and b
	sll $v0, $v0, 5				# shift the sum the necessary bits left
	or $s2, $s2, $v0			# turn on the bit of the final sum if necessary
	move $a2, $v1				# copy the cout to cin for the next adder call
	srl $s0, $s0, 1				# shift a right one to add the next bit
	srl $s1, $s1, 1				# shift b right one to add the next bit
	move $a0, $s0				# a0 = a for bitadd
	move $a1, $s1				# a1 = b for bitadd
	jal BitAdd					# run the adder on the first bits of a and b
	sll $v0, $v0, 6				# shift the sum the necessary bits left
	or $s2, $s2, $v0			# turn on the bit of the final sum if necessary
	move $a2, $v1				# copy the cout to cin for the next adder call
	srl $s0, $s0, 1				# shift a right one to add the next bit
	srl $s1, $s1, 1				# shift b right one to add the next bit
	move $a0, $s0				# a0 = a for bitadd
	move $a1, $s1				# a1 = b for bitadd
	jal BitAdd					# run the adder on the first bits of a and b
	sll $v0, $v0, 7				# shift the sum the necessary bits left
	or $s2, $s2, $v0			# turn on the bit of the final sum if necessary
	move $a2, $v1				# copy the cout to cin for the next adder call
	srl $s0, $s0, 1				# shift a right one to add the next bit
	srl $s1, $s1, 1				# shift b right one to add the next bit
	move $a0, $s0				# a0 = a for bitadd
	move $a1, $s1				# a1 = b for bitadd
	jal BitAdd					# run the adder on the first bits of a and b
	sll $v0, $v0, 8				# shift the sum the necessary bits left
	or $s2, $s2, $v0			# turn on the bit of the final sum if necessary
	move $a2, $v1				# copy the cout to cin for the next adder call
	srl $s0, $s0, 1				# shift a right one to add the next bit
	srl $s1, $s1, 1				# shift b right one to add the next bit
	move $a0, $s0				# a0 = a for bitadd
	move $a1, $s1				# a1 = b for bitadd
	jal BitAdd					# run the adder on the first bits of a and b
	sll $v0, $v0, 9				# shift the sum the necessary bits left
	or $s2, $s2, $v0			# turn on the bit of the final sum if necessary
	move $a2, $v1				# copy the cout to cin for the next adder call
	srl $s0, $s0, 1				# shift a right one to add the next bit
	srl $s1, $s1, 1				# shift b right one to add the next bit
	move $a0, $s0				# a0 = a for bitadd
	move $a1, $s1				# a1 = b for bitadd
	jal BitAdd					# run the adder on the first bits of a and b
	sll $v0, $v0, 10			# shift the sum the necessary bits left
	or $s2, $s2, $v0			# turn on the bit of the final sum if necessary
	move $a2, $v1				# copy the cout to cin for the next adder call
	srl $s0, $s0, 1				# shift a right one to add the next bit
	srl $s1, $s1, 1				# shift b right one to add the next bit
	move $a0, $s0				# a0 = a for bitadd
	move $a1, $s1				# a1 = b for bitadd
	jal BitAdd					# run the adder on the first bits of a and b
	sll $v0, $v0, 11			# shift the sum the necessary bits left
	or $s2, $s2, $v0			# turn on the bit of the final sum if necessary
	move $a2, $v1				# copy the cout to cin for the next adder call
	srl $s0, $s0, 1				# shift a right one to add the next bit
	srl $s1, $s1, 1				# shift b right one to add the next bit
	move $a0, $s0				# a0 = a for bitadd
	move $a1, $s1				# a1 = b for bitadd
	jal BitAdd					# run the adder on the first bits of a and b
	sll $v0, $v0, 12			# shift the sum the necessary bits left
	or $s2, $s2, $v0			# turn on the bit of the final sum if necessary
	move $a2, $v1				# copy the cout to cin for the next adder call
	srl $s0, $s0, 1				# shift a right one to add the next bit
	srl $s1, $s1, 1				# shift b right one to add the next bit
	move $a0, $s0				# a0 = a for bitadd
	move $a1, $s1				# a1 = b for bitadd
	jal BitAdd					# run the adder on the first bits of a and b
	sll $v0, $v0, 13			# shift the sum the necessary bits left
	or $s2, $s2, $v0			# turn on the bit of the final sum if necessary
	move $a2, $v1				# copy the cout to cin for the next adder call
	srl $s0, $s0, 1				# shift a right one to add the next bit
	srl $s1, $s1, 1				# shift b right one to add the next bit
	move $a0, $s0				# a0 = a for bitadd
	move $a1, $s1				# a1 = b for bitadd
	jal BitAdd					# run the adder on the first bits of a and b
	sll $v0, $v0, 14			# shift the sum the necessary bits left
	or $s2, $s2, $v0			# turn on the bit of the final sum if necessary
	move $a2, $v1				# copy the cout to cin for the next adder call
	srl $s0, $s0, 1				# shift a right one to add the next bit
	srl $s1, $s1, 1				# shift b right one to add the next bit
	move $a0, $s0				# a0 = a for bitadd
	move $a1, $s1				# a1 = b for bitadd
	jal BitAdd					# run the adder on the first bits of a and b
	sll $v0, $v0, 15			# shift the sum the necessary bits left
	or $s2, $s2, $v0			# turn on the bit of the final sum if necessary
	move $a2, $v1				# copy the cout to cin for the next adder call
	srl $s0, $s0, 1				# shift a right one to add the next bit
	srl $s1, $s1, 1				# shift b right one to add the next bit
	move $a0, $s0				# a0 = a for bitadd
	move $a1, $s1				# a1 = b for bitadd
	jal BitAdd					# run the adder on the first bits of a and b
	sll $v0, $v0, 16			# shift the sum the necessary bits left
	or $s2, $s2, $v0			# turn on the bit of the final sum if necessary
	move $a2, $v1				# copy the cout to cin for the next adder call
	srl $s0, $s0, 1				# shift a right one to add the next bit
	srl $s1, $s1, 1				# shift b right one to add the next bit
	move $a0, $s0				# a0 = a for bitadd
	move $a1, $s1				# a1 = b for bitadd
	jal BitAdd					# run the adder on the first bits of a and b
	sll $v0, $v0, 17			# shift the sum the necessary bits left
	or $s2, $s2, $v0			# turn on the bit of the final sum if necessary
	move $a2, $v1				# copy the cout to cin for the next adder call
	srl $s0, $s0, 1				# shift a right one to add the next bit
	srl $s1, $s1, 1				# shift b right one to add the next bit
	move $a0, $s0				# a0 = a for bitadd
	move $a1, $s1				# a1 = b for bitadd
	jal BitAdd					# run the adder on the first bits of a and b
	sll $v0, $v0, 18			# shift the sum the necessary bits left
	or $s2, $s2, $v0			# turn on the bit of the final sum if necessary
	move $a2, $v1				# copy the cout to cin for the next adder call
	srl $s0, $s0, 1				# shift a right one to add the next bit
	srl $s1, $s1, 1				# shift b right one to add the next bit
	move $a0, $s0				# a0 = a for bitadd
	move $a1, $s1				# a1 = b for bitadd
	jal BitAdd					# run the adder on the first bits of a and b
	sll $v0, $v0, 19			# shift the sum the necessary bits left
	or $s2, $s2, $v0			# turn on the bit of the final sum if necessary
	move $a2, $v1				# copy the cout to cin for the next adder call
	srl $s0, $s0, 1				# shift a right one to add the next bit
	srl $s1, $s1, 1				# shift b right one to add the next bit
	move $a0, $s0				# a0 = a for bitadd
	move $a1, $s1				# a1 = b for bitadd
	jal BitAdd					# run the adder on the first bits of a and b
	sll $v0, $v0, 20			# shift the sum the necessary bits left
	or $s2, $s2, $v0			# turn on the bit of the final sum if necessary
	move $a2, $v1				# copy the cout to cin for the next adder call
	srl $s0, $s0, 1				# shift a right one to add the next bit
	srl $s1, $s1, 1				# shift b right one to add the next bit
	move $a0, $s0				# a0 = a for bitadd
	move $a1, $s1				# a1 = b for bitadd
	jal BitAdd					# run the adder on the first bits of a and b
	sll $v0, $v0, 21			# shift the sum the necessary bits left
	or $s2, $s2, $v0			# turn on the bit of the final sum if necessary
	move $a2, $v1				# copy the cout to cin for the next adder call
	srl $s0, $s0, 1				# shift a right one to add the next bit
	srl $s1, $s1, 1				# shift b right one to add the next bit
	move $a0, $s0				# a0 = a for bitadd
	move $a1, $s1				# a1 = b for bitadd
	jal BitAdd					# run the adder on the first bits of a and b
	sll $v0, $v0, 22			# shift the sum the necessary bits left
	or $s2, $s2, $v0			# turn on the bit of the final sum if necessary
	move $a2, $v1				# copy the cout to cin for the next adder call
	srl $s0, $s0, 1				# shift a right one to add the next bit
	srl $s1, $s1, 1				# shift b right one to add the next bit
	move $a0, $s0				# a0 = a for bitadd
	move $a1, $s1				# a1 = b for bitadd
	jal BitAdd					# run the adder on the first bits of a and b
	sll $v0, $v0, 23			# shift the sum the necessary bits left
	or $s2, $s2, $v0			# turn on the bit of the final sum if necessary
	move $a2, $v1				# copy the cout to cin for the next adder call
	srl $s0, $s0, 1				# shift a right one to add the next bit
	srl $s1, $s1, 1				# shift b right one to add the next bit
	move $a0, $s0				# a0 = a for bitadd
	move $a1, $s1				# a1 = b for bitadd
	jal BitAdd					# run the adder on the first bits of a and b
	sll $v0, $v0, 24			# shift the sum the necessary bits left
	or $s2, $s2, $v0			# turn on the bit of the final sum if necessary
	move $a2, $v1				# copy the cout to cin for the next adder call
	srl $s0, $s0, 1				# shift a right one to add the next bit
	srl $s1, $s1, 1				# shift b right one to add the next bit
	move $a0, $s0				# a0 = a for bitadd
	move $a1, $s1				# a1 = b for bitadd
	jal BitAdd					# run the adder on the first bits of a and b
	sll $v0, $v0, 25			# shift the sum the necessary bits left
	or $s2, $s2, $v0			# turn on the bit of the final sum if necessary
	move $a2, $v1				# copy the cout to cin for the next adder call
	srl $s0, $s0, 1				# shift a right one to add the next bit
	srl $s1, $s1, 1				# shift b right one to add the next bit
	move $a0, $s0				# a0 = a for bitadd
	move $a1, $s1				# a1 = b for bitadd
	jal BitAdd					# run the adder on the first bits of a and b
	sll $v0, $v0, 26			# shift the sum the necessary bits left
	or $s2, $s2, $v0			# turn on the bit of the final sum if necessary
	move $a2, $v1				# copy the cout to cin for the next adder call
	srl $s0, $s0, 1				# shift a right one to add the next bit
	srl $s1, $s1, 1				# shift b right one to add the next bit
	move $a0, $s0				# a0 = a for bitadd
	move $a1, $s1				# a1 = b for bitadd
	jal BitAdd					# run the adder on the first bits of a and b
	sll $v0, $v0, 27			# shift the sum the necessary bits left
	or $s2, $s2, $v0			# turn on the bit of the final sum if necessary
	move $a2, $v1				# copy the cout to cin for the next adder call
	srl $s0, $s0, 1				# shift a right one to add the next bit
	srl $s1, $s1, 1				# shift b right one to add the next bit
	move $a0, $s0				# a0 = a for bitadd
	move $a1, $s1				# a1 = b for bitadd
	jal BitAdd					# run the adder on the first bits of a and b
	sll $v0, $v0, 28			# shift the sum the necessary bits left
	or $s2, $s2, $v0			# turn on the bit of the final sum if necessary
	move $a2, $v1				# copy the cout to cin for the next adder call
	srl $s0, $s0, 1				# shift a right one to add the next bit
	srl $s1, $s1, 1				# shift b right one to add the next bit
	move $a0, $s0				# a0 = a for bitadd
	move $a1, $s1				# a1 = b for bitadd
	jal BitAdd					# run the adder on the first bits of a and b
	sll $v0, $v0, 29			# shift the sum the necessary bits left
	or $s2, $s2, $v0			# turn on the bit of the final sum if necessary
	move $a2, $v1				# copy the cout to cin for the next adder call
	srl $s0, $s0, 1				# shift a right one to add the next bit
	srl $s1, $s1, 1				# shift b right one to add the next bit
	move $a0, $s0				# a0 = a for bitadd
	move $a1, $s1				# a1 = b for bitadd
	jal BitAdd					# run the adder on the first bits of a and b
	sll $v0, $v0, 30			# shift the sum the necessary bits left
	or $s2, $s2, $v0			# turn on the bit of the final sum if necessary
	move $a2, $v1				# copy the cout to cin for the next adder call
	srl $s0, $s0, 1				# shift a right one to add the next bit
	srl $s1, $s1, 1				# shift b right one to add the next bit
	move $a0, $s0				# a0 = a for bitadd
	move $a1, $s1				# a1 = b for bitadd
	jal BitAdd					# run the adder on the first bits of a and b
	sll $v0, $v0, 31			# shift the sum the necessary bits left
	or $s2, $s2, $v0			# turn on the bit of the final sum if necessary
	move $a2, $v1				# copy the cout to cin for the next adder call
	srl $s0, $s0, 1				# shift a right one to add the next bit
	srl $s1, $s1, 1				# shift b right one to add the next bit
AddEnd:
	move $a0, $s4				# sign bits in a0 for Overflow
	move $a1, $s2				# sum in a1 for Overflow
	jal Overflow				# call Overflow to check for overflow
	move $v1, $v0				# Overflow puts its return in v0, move it to v1
	move $v0, $s2				# v0 = sum
	lw $ra, 0($sp)				# reload $ra
	lw $s0, 4($sp)				# reload $s0
	lw $s1, 8($sp)				# reload $s1
	lw $s2, 12($sp)				# reload $s2
	lw $s3, 16($sp)				# reload $s3
	lw $s4, 20($sp)				# reload $s4
	addi $sp, $sp, 24			# return 5 words to stack
	jr $ra						# return to calling code
# end of Add

# Detect overflow, takes two args, first (in a0) is a word with two bits
# signifying the sign of the two integers summed (so if two negative numbers
# are summed, the bit pattern 000...0011 is passed, etc.) and the second
# (in a1) is the sum.
# returns 1 in v0 if overflow occurred
Overflow:
	srl $a1, $a1, 31			# only need first bit of sum
	nor $t0, $zero, $zero		# set t0 to all 1's
	srl $t0, $t0, 30			# shift all 1's s.t. we end with bit pattern 11
	and $a0, $a0, $t0			# mask all but last two (LSB) bits
	beq $a0, $zero, OverflowPossiblePositive # both add operands were positive
	beq $a0, $t0, OverflowPossibleNegative	# both add operands were negative
	move $v0, $zero				# otherwise oflow can't have occurred
	j OverflowReturn
OverflowPossiblePositive:
	srl $t0, $t0, 1				# only need bit pattern 1 to test for sum<0
	beq $a1, $t0, OverflowOccurred	# if sum's negative, oflow happened
	j OverflowReturn			# go set v0 to 1 and return
OverflowPossibleNegative:
	beq $a1, $zero, OverflowOccurred	# if sum's positive, oflow happened
	j OverflowReturn			# go set v0 to 1 and return
OverflowOccurred:
	li $v0, 1					# load v0 with 1 to signify overflow occured
OverflowReturn:
	jr $ra						# return to calling code
# end of Overflow

# Subtract a0-a1 using two's complement
# difference in v0
Sub:
	addi $sp, $sp, -8			# get 2 words on stack
	sw $ra, 0($sp)				# store return addr
	sw $s0, 4($sp)				# store s0
	move $s0, $a0				# save the first arg
	move $a0, $a1				# move the second arg to a0 for Negate
	jal Negate					# negate the second arg
	move $a0, $s0				# move first arg back to a0 for Add
	move $a1, $v0				# put the negated number in a1 for Add
	jal Add						# add the numbers
	lw $ra, 0($sp)				# restore ra
	lw $s0, 4($sp)				# restore s0
	addi $sp, $sp, 8			# return 2 words to stack
	jr $ra						# return to calling code
# end of Sub

# negates a number represented using two's complement (invert and add 1)
# number in a0, negation in v0
Negate:
	addi $sp, $sp, -4			# get a word from stack
	sw $ra, 0($sp)				# store ra
	nor $t0, $zero, $zero		# set to all 1's
	xor $a0, $a0, $t0			# xor will set all bits 1 to 0 and 0 to 1
	li $a1, 1					# load 1 to a1, for Add
	jal Add						# add 1 to a0, stores in v0
	lw $ra, 0($sp)				# restore ra
	addi $sp, $sp, 4			# return word to stack
	jr $ra						# return to calling code negated val in v0
# end of Negate

# sets v0 to 1 if a0 < a1 (so a0 - a1 is negative)
Slt:
	addi $sp, $sp, -4			# get a word from stack
	sw $ra, 0($sp)				# store ra
	jal Sub						# do a0 - a1
	srl $v0, $v0, 31			# move the msb to the lsb pos, it's 1 or 0
	lw $ra, 0($sp)				# restore ra
	addi $sp, $sp, 4			# return word to stack
	jr $ra						# return to calling code
#end of Slt

# print word in a0 as an integer
PrintInt:
	li $v0, 1
	syscall
	jr $ra

# print string at address in a0
PrintStr:
	li $v0, 4
	syscall
	jr $ra

# An ALU with the opcodes described by the instructions: array above
# a0 has the opcode, a1 and a2 have the operators
# v0 has the result, v1 is zero unless an arithmetic overflow occurred 
ALU:
	addi $sp, $sp, -4			# get a word on the stack
	sw $ra, 0($sp)				# store the return address
	move $v1, $zero				# default the oflow value to 0
	la $t1, instructions		# load the instructions array addr
	move $t0, $a0				# put the opcode in t0
	move $a0, $a1				# move the first 'real' arg to a0
	move $a1, $a2				# move the second 'real' arg to a1
	add $t0, $t0, $t0			# multiply the opcode by 4
	add $t0, $t0, $t0			# cont'd
	add $t0, $t0, $t1			# add the opcode offset to the base inst addr
	lw $t0, 0($t0)				# load the instruction address from the array
	jalr $t0					# jump and link to the requested instruction
ALUreturn:
	lw $ra, 0($sp)				# reload the return address
	addi $sp, $sp, 4			# return the word to the stack
	jr $ra						# return to calling code
	
main:							# example, test procedure
	li $a0, 2					# opcode
	li $a1, 100					# a
	li $a2, 1000				# b
	jal ALU						# call ALU with sum code
	move $a0, $v0				# move sum to be printed
	jal PrintInt				# print sum
	la $a0, OverflowNotice		# load address to overflow desc
	jal PrintStr				# print the overflow bit desc
	move $a0, $v1				# move the oflow bit to be printed
	jal PrintInt				# print the oflow bit
Quit:
	li $v0, 10					# load quit code
	syscall						# quit

