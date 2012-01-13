import datetime
from django.forms import widgets
from django.db.models.fields import BLANK_CHOICE_DASH
import afap.models

class YearInput(widgets.TextInput):
    '''Input widget designed to display and take year values'''
    def render(self, name, value, attrs=None):
        if isinstance(value, datetime.date):
            value = value.year
        return super(YearInput, self).render(name, value, attrs)

class YearSelect(widgets.Select):
    '''Input widget giving a list of years'''
    def render(self, name, value, attrs=None, choices=()):
        choices = BLANK_CHOICE_DASH[:]
        if not isinstance(value, datetime.date):
            try:
                value = datetime.date(int(value), 1, 1)
            except (TypeError, ValueError):
                value = None
        for year in afap.models.Year.objects.all().order_by('-date'):
            choices.append((year.date.year, year.date.range,))
            if isinstance(value, datetime.date) and value.year == year.date.year:
                value = year.date.year
        return super(YearSelect, self).render(name, value, attrs, choices)

