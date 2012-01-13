#include <sys/types.h>
#include <stdio.h>
#include <unistd.h>
#include <stdlib.h>

int main(int argc, char **argv) {
    int n;
    if (argc != 2) {
        goto usage;
    } else {
        n = atoi(argv[1]);
        if (n < 0) {
            goto usage;
        }
    }

    int i = 0;
    unsigned int prev = 0;
    unsigned int sum = 1;

    pid_t pid = fork();
    if (pid == 0) { // child process
        printf("%u ", prev);
        if (n > 0) {
            printf("%u ", sum);
            i++;
        }
        for (; i < n; ++i) {
            printf("%u ", prev + sum);
            unsigned int temp = prev;
            prev = sum;
            sum += temp;
        }
    } else if (pid > 0) { // parent process
        wait(NULL);
    } else { // error
        printf("fork() failed\n");
        abort();
    }
    return 0;
usage:
    printf("usage: %s N >= 0, computes fibonacci sequence through N\n", argv[0]);
    return 0;
}
