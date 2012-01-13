def fib(n):
	prev = 0
	curr = 1
	if n < 2:
		return 1
	while n >= 2:
		temp = prev + curr
		prev = curr
		curr = temp
		n -= 1
	return curr
