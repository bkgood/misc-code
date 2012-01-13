#include <stdio.h>

long long fib(long long n)
{
	long long prev = 0;
	long long curr = 1;
	long long temp;
	if (n < 2) {
		return 1;
	}
	while (n >= 2) {
		temp = prev + curr;
		prev = curr;
		curr = temp;
		n--;
	}
	return curr;
}

void main(void)
{
	long long n = 100000000;
	printf("the %lldth fib number is %lld\n", n, fib(n));
}
