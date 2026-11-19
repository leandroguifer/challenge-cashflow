insert into daily_balances (store_id, business_date, total_credits, total_debits, entries_count, updated_at)
select
    stores.store_id,
    days.business_date::date,
    round((random() * 5000 + 1000)::numeric, 2),
    round((random() * 2000 + 200)::numeric, 2),
    (random() * 40 + 5)::int,
    now()
from (values ('LOJA01'), ('LOJA02'), ('LOJA03')) as stores(store_id)
cross join generate_series(current_date - 400, current_date, interval '1 day') as days(business_date)
on conflict (store_id, business_date) do nothing;
