import datetime

class RangedDate (datetime.date):
    def __new__(cls, *args, **kwargs):
        if len(args) > 0 and isinstance(args[0], datetime.date):
            (y, m, d) = (args[0].year, args[0].month, args[0].day)
            self = super(RangedDate, cls).__new__(cls, y, m, d)
        else:
            self = super(RangedDate, cls).__new__(cls, *args, **kwargs)
        self.__init__(*args, **kwargs)
        return self

    range = property(lambda self: u'%d - %d' % (self.year, self.year + 1,))

