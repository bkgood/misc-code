;; William Good
;; CS3363
;; Due December 9, 2011
;; Program 5
;; December 7, 2011
;;
;; run with clisp <pgm5.cl

(defun ins (x l)
  "Inserts an element x into a sorted list of numbers l"
  (cond
    ((not l) (list x))                    ; if l is nil, return (x)
    ((< x (first l)) (cons x l))          ; if x < head(l), return x::l
    (t (cons (first l) (ins x (rest l)))) ; otherwise, return head(l)::ins(x rest(l))
    )
  )

(defun insort (l)
  "Sorts a list of numbers l using insertion sort"
  (cond
    ((not l) nil)                         ; if l is nil, return nil
    (t (ins (first l) (insort (rest l)))) ; otherwise, insert head(l) into a sorted rest(l)
    )
  )

(insort nil)
(insort '(1.0))
(insort '(10.1 20.20))
(insort '(1.0 1.0 1.0))
(setq L '(3.5 7.9 1.5 10.1 5.3 8.1 6.1 0.1 2.5 4.2 1.5 7.8))
(insort L)

(bye) ; had to add this to get clisp on csx to stop reading from standard input
