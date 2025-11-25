// El Mansour Syndic Manager - Unpaid House Notifications Edge Function
// Deploy to Supabase Edge Functions
// Schedule: Daily at 9 AM (cron: 0 9 * * *)

import { serve } from "https://deno.land/std@0.168.0/http/server.ts"
import { createClient } from 'https://esm.sh/@supabase/supabase-js@2'

const corsHeaders = {
  'Access-Control-Allow-Origin': '*',
  'Access-Control-Allow-Headers': 'authorization, x-client-info, apikey, content-type',
}

serve(async (req) => {
  if (req.method === 'OPTIONS') {
    return new Response('ok', { headers: corsHeaders })
  }

  try {
    const supabaseUrl = Deno.env.get('SUPABASE_URL')!
    const supabaseServiceKey = Deno.env.get('SUPABASE_SERVICE_ROLE_KEY')!
    const supabase = createClient(supabaseUrl, supabaseServiceKey)

    // Get current month
    const currentMonth = new Date().toISOString().slice(0, 7) // YYYY-MM

    // Find unpaid houses for current month
    const { data: unpaidPayments, error } = await supabase
      .from('payments')
      .select('*, houses(*)')
      .eq('month', currentMonth)
      .eq('status', 'Unpaid')

    if (error) throw error

    // Create notifications for unpaid houses
    const notifications = unpaidPayments.map(payment => ({
      user_id: null, // All users
      type: 'UnpaidHouse',
      title: 'Maison Non Payée',
      message: `La maison ${payment.house_code} n'a pas encore payé pour ${currentMonth}`,
      related_entity_type: 'Payment',
      related_entity_id: payment.id,
      priority: 'High'
    }))

    if (notifications.length > 0) {
      const { error: notifError } = await supabase
        .from('notifications')
        .insert(notifications)

      if (notifError) throw notifError
    }

    return new Response(
      JSON.stringify({
        success: true,
        notifications_created: notifications.length,
        timestamp: new Date().toISOString()
      }),
      { headers: { ...corsHeaders, 'Content-Type': 'application/json' } },
    )
  } catch (error) {
    return new Response(
      JSON.stringify({ error: error.message }),
      { 
        status: 500,
        headers: { ...corsHeaders, 'Content-Type': 'application/json' } 
      },
    )
  }
})

