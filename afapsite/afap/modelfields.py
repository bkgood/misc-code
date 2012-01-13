from django.db import models
from south.modelsinspector import add_introspection_rules
import datetime
import afap.formfields
from afap.util.rangeddate import RangedDate

class YearField(models.DateField):
    '''Represents a year as a date with month and day set to 1'''
    # ensures to_python will always be called
    __metaclass__ = models.SubfieldBase

    def __init__(self, selectable=True, *args, **kwargs):
        self.selectable = selectable
        super(YearField, self).__init__(*args, **kwargs)

    def to_python(self, value):
        if isinstance(value, (int, long, float)):
            return RangedDate(int(value), 1, 1)
        if isinstance(value, basestring):
            try:
                return RangedDate(int(value.strip()), 1, 1)
            except:
                pass
        superstuff = super(YearField, self).to_python(value)
        if isinstance(superstuff, datetime.date):
            return RangedDate(superstuff.year, 1, 1)
        return None

    def formfield(self, **kwargs):
        defaults = {
            'form_class': afap.formfields.YearField,
            'selectable': self.selectable,
        }
        defaults.update(kwargs)
        return super(YearField, self).formfield(**defaults)

add_introspection_rules([], ["^afap\.modelfields\.YearField"])

