from decimal import Decimal

def redirect_to_application(app):
    if app.applicationfile_set.filter(replaces_application=True):
        return True
    return False

def sum_line_items(line_items):
    sum = Decimal(0)
    for line_item in line_items:
        if line_item.income:
            sum -= line_item.amount
        else:
            sum += line_item.amount
    return sum
