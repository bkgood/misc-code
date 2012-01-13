import datetime
from django import forms
import afap.widgets
from afap.util.rangeddate import RangedDate

class YearField (forms.IntegerField):
    '''Form field that takes a year value'''
    def __init__(self, selectable=True, **kwargs):
        if selectable:
            widget = afap.widgets.YearSelect
        else:
            widget = afap.widgets.YearInput
        defaults = {
            'min_value': datetime.date(2000, 1, 1),
            'widget': widget,
        }
        kwargs.update(defaults)
        super(YearField, self).__init__(**kwargs)

    def to_python(self, value):
        if isinstance(value, datetime.date):
            return RangedDate(value.year, 1, 1)
        elif isinstance(value, (int, long, float)):
            return RangedDate(int(value), 1, 1)
        year = super(YearField, self).to_python(value)
        if year:
            return RangedDate(year, 1, 1)
        return None

    def clean(self, value):
        value = self.to_python(value)
        self.validate(value)
        self.run_validators(value)
        return value

