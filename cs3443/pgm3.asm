.data

.text
.globl main

# a: a0, b: a1, cin: a2
# sum: v0 cout: v1
# adds first bit (LSB) of a and b
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

# adds a and b
# takes a in $a0 and b in $a1, returns sum in $v1 and carry-over (to be overflow) bit in $v1
Add:
	addi $sp, $sp, -20			# get 5 words on stack
	sw $ra, 0($sp)				# store return address
	sw $s0, 4($sp)				# s0 = a
	sw $s1, 8($sp)				# s1 = b
	sw $s2, 12($sp)				# s2 = sum
	sw $s3, 16($sp)				# s3 = counter, used in shifts
	move $s0, $a0				# store a
	move $s1, $a1				# store b
	li $s2, 0					# load the sum with 0
	li $s3, 0					# load cout with 0
	move $a2, $zero				# initialize cin to the adder to 0
AddNotZero:
	move $a0, $s0				# a0 = a
	move $a1, $s1				# a1 = b
	jal BitAdd					# run the adder on the first bits of a and b
	sllv $v0, $v0, $s3			# shift the sum the necessary bits left
	or $s2, $s2, $v0			# turn on the bit of the final sum if necessary
	srl $s0, $s0, 1				# shift a right one, for the next bit to be added
	srl $s1, $s1, 1				# shift b right one, for the next bit to be added
	addi $s3, $s3, 1			# add 1 to the shift counter, for shifting the sum
	move $a2, $v1				# copy the cout to cin for the next call to the adder
	bne $s0, $zero, AddNotZero	# if a is not zero, add again
	bne $s1, $zero, AddNotZero	# if b is not zero, add again
	li $t0, 32					# load 32, if shift is 32 bits then problems occur
	beq $s3, $t0, AddEnd		# stop the iteration in this case
	bne $a2, $zero, AddNotZero	# if carry is not zero, add again
AddEnd:
	move $v0, $s2				# v0 = sum
	lw $ra, 0($sp)				# reload $ra
	lw $s0, 4($sp)				# reload $s0
	lw $s1, 8($sp)				# reload $s1
	lw $s2, 12($sp)				# reload $s2
	lw $s3, 16($sp)				# reload $s3
	addi $sp, $sp, 20			# return 5 words to stack
	jr $ra						# return to calling code

# print word in a0 as an integer
PrintInt:
	li $v0, 1
	syscall
	jr $ra

main:
	li $a0, 1000				# a
	li $a1, -1000				# b
	jal Add
	move $a0, $v0				# print sum
	jal PrintInt
Quit:
	li $v0, 10
	syscall
	nop
